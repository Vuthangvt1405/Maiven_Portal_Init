# Resource-oriented API routes

Controllers are grouped by the resource/service they expose. Roles authorize operations; they do not define controller boundaries or URL prefixes.

## Route map

| Resource | Method | URL | Access |
|---|---|---|---|
| Authentication | `POST` | `/api/auth/register` | Anonymous |
| Authentication | `POST` | `/api/auth/login` | Anonymous |
| Authentication | `POST` | `/api/auth/admin/login` | Anonymous request; credentials must belong to an admin |
| Current user | `GET` | `/api/users/me` | Authenticated user |
| Current user | `PUT` | `/api/users/profile` | Authenticated user |
| Teachers | `POST` | `/api/teachers` | Admin |
| Academic years | `POST` | `/api/academic-years` | Admin |
| Academic years | `GET` | `/api/academic-years` | Admin |
| Academic years | `PUT` | `/api/academic-years/{academicYearId}` | Admin |
| Semesters | `POST` | `/api/semesters` | Admin |
| Semesters | `GET` | `/api/semesters` | Admin |
| Semesters | `PUT` | `/api/semesters/{semesterId}` | Admin |
| Courses | `POST` | `/api/courses` | Admin |
| Courses | `GET` | `/api/courses` | Public |
| Courses | `GET` | `/api/courses/{courseId}` | Public |
| Courses | `PUT` | `/api/courses/{courseId}` | Admin |
| Courses | `DELETE` | `/api/courses/{courseId}` | Admin |

## Current-user behavior

`GET /api/users/me` returns the authenticated token's user ID, email, singular role code, and `roleUserId` (`USER_ROLES.id`). `PUT /api/users/profile` updates only the authenticated user's editable profile fields. Student, teacher, and admin accounts use these same endpoints.

```json
{
  "id": 1,
  "email": "user@example.com",
  "role": "STUDENT",
  "roleUserId": 42
}
```

Profile updates cannot change the user's ID, email, password, role, role assignment, or deletion state. Identity values always come from the authenticated JWT.

## Removed routes

Role-prefixed routes such as `/api/admin/academic-years`, `/api/admin/semesters`, `/api/admin/teachers`, `/api/admin/me`, `/api/students/*`, and role-specific `/api/teachers/me` or `/api/teachers/profile` are no longer mapped.
