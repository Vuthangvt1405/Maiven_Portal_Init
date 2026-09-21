# Admin academic-year workflow

## Endpoints

All routes require a valid JWT containing the `ADMIN` role.

| Method | URL | Success | Purpose |
|---|---|---:|---|
| `POST` | `/api/academic-years` | `201` | Create an academic year |
| `GET` | `/api/academic-years` | `200` | List all non-deleted academic years |
| `PUT` | `/api/academic-years/{academicYearId}` | `200` | Fully replace an academic year |

The list is not paginated. It is ordered by `startDate` descending and then `id` descending. Deleted records are excluded and `isDeleted` is never exposed.

## Request body

`POST` and `PUT` use separate request DTOs but require the same complete body:

```json
{
  "name": "2026-2027",
  "startDate": "2026-09-01",
  "endDate": "2027-06-30",
  "status": "ACTIVE"
}
```

- `name` is trimmed and must contain 1-200 characters.
- `startDate` and `endDate` are required ISO dates; the end cannot precede the start.
- `status` is required and must be `ACTIVE` or `COMPLETED`.
- `PUT` is a full replacement. Missing fields are invalid.

## Response

```json
{
  "id": 12,
  "name": "2026-2027",
  "startDate": "2026-09-01",
  "endDate": "2027-06-30",
  "status": "ACTIVE",
  "createdAt": "2026-09-20T14:00:00Z",
  "updatedAt": "2026-09-20T14:00:00Z"
}
```

`POST` and `PUT` return one object. `GET` returns a direct array of these objects.

## Validation and conflicts

```text
HTTP request
  -> ADMIN authorization
  -> request DTO validation
  -> AcademicYearService trims and validates input
  -> conflict and semester-boundary checks
  -> insert/update
  -> AcademicYearResponse
```

A non-deleted academic year cannot:

- duplicate another year name (case-insensitive),
- overlap another year's inclusive date range, or
- become a second `ACTIVE` year.

An update also cannot shrink the year so that a non-deleted child semester falls outside the proposed range. The current academic year is excluded from its own conflict checks.

| Status | Meaning |
|---:|---|
| `400` | Invalid/missing fields or `startDate` after `endDate` |
| `401` | Missing or invalid authentication |
| `403` | Authenticated account does not have the `ADMIN` role |
| `404` | Update target is missing or soft-deleted |
| `409` | Duplicate name, overlapping range, second active year, or excluded semester dates |

Filtered unique indexes enforce non-deleted name uniqueness and the single active year rule under concurrent writes. The service translates those database violations to the same stable `409` responses.
