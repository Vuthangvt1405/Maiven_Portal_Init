using log4net;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class TeacherService(
	AuthRepository authRepository,
	IPasswordHasher<User> passwordHasher)
{
	private static readonly ILog Logger = LogManager.GetLogger(typeof(TeacherService));

	public async Task<AuthUserResponse> CreateTeacherAsync(
		CreateTeacherRequest request,
		CancellationToken cancellationToken)
	{
		var normalizedEmail = request.Email.Trim().ToLowerInvariant();

		if (await authRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
		{
			throw new ConflictException("An account with this email already exists.");
		}

		var user = new User
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

		var role = await authRepository.GetRoleByCodeAsync(
			SystemRoles.Teacher.Code,
			cancellationToken)
			?? throw new InvalidOperationException(
				$"The required {SystemRoles.Teacher.Code} role is not configured.");

		user.PasswordHash = passwordHash;
		user.UserRoles.Add(new UserRole
		{
			RoleId = role.Id
		});

		try
		{
			await authRepository.AddUserAsync(user, cancellationToken);
		}
		catch (DbUpdateException exception)
			when (AuthRepository.IsUniqueConstraintViolation(exception))
		{
			throw new ConflictException("An account with this email already exists.", exception);
		}

		var account = new AuthAccount
		{
			User = user,
			PasswordHash = user.PasswordHash,
			RoleAssignments =
			[
				new AuthRoleAssignment
				{
					RoleUserId = user.UserRoles.Single().Id,
					RoleCode = role.Code
				}
			]
		};

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
		Logger.Info($"Teacher created UserId={response.Id} Email={LogFormat.FormatValue(response.Email)}");
		return response;
	}

	private static string? NormalizeOptional(string? value) =>
		string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
