using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Microsoft.AspNetCore.Identity;

namespace Maiven_Portal_Managment.Services;

public sealed class TeacherService(
	AuthRepository authRepository,
	IPasswordHasher<UserModel> passwordHasher,
	ActionLogService actionLogService)
{
	public async Task<AuthUserResponse> CreateTeacherAsync(
		CreateTeacherRequest request,
		CancellationToken cancellationToken)
	{
		using var operation = actionLogService.Begin<TeacherService>(
			"Service",
			nameof(CreateTeacherAsync));
		var normalizedEmail = request.Email.Trim().ToLowerInvariant();

		if (await authRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
		{
			throw new ConflictException("An account with this email already exists.");
		}

		var user = new UserModel
		{
			Email = normalizedEmail,
			FullName = request.FullName.Trim(),
			DateOfBirth = request.DateOfBirth,
			Gender = request.Gender,
			Phone = NormalizeOptional(request.Phone),
			Address = NormalizeOptional(request.Address),
			AvatarUrl = NormalizeOptional(request.AvatarUrl)
		};
		var passwordHash = passwordHasher.HashPassword(user, request.Password);
		var account = await authRepository.CreateTeacherAsync(
			user,
			passwordHash,
			cancellationToken);

		var roleAssignment = account.RoleAssignments.Single();

		var response = new AuthUserResponse
		{
			Id = account.User.Id,
			Email = account.User.Email,
			FullName = account.User.FullName,
			DateOfBirth = account.User.DateOfBirth,
			Gender = account.User.Gender,
			Phone = account.User.Phone,
			Address = account.User.Address,
			AvatarUrl = account.User.AvatarUrl,
			Role = roleAssignment.RoleCode,
			RoleUserId = roleAssignment.RoleUserId
		};
		operation.Complete(("UserId", response.Id), ("Role", response.Role));
		return response;
	}

	private static string? NormalizeOptional(string? value) =>
		string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
