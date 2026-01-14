# Ordning

An inventory system for organizing items and locations.

## Project Structure

- **Backend**: `Ordning.Server/` - C# ASP.NET Core API
- **Frontend**: `ordning.client/` - TypeScript React application

## Specifications

See [spec.md](spec.md) for detailed requirements and feature specifications.

## Reference Project

The `betapet-admin/` folder is temporarily included as a reference project for inspiration. It can be used as guidance when writing code for both backend and frontend:

- **Backend**: Follow the domain-driven folder structure (e.g., `Auth/`, `Users/`, etc.)
- **Frontend**: Reuse components and patterns from `betapet-admin/betapetadmin.client/`

## Frontend Development

- **Type Generation**: Frontend types are auto-generated from the backend Swagger/OpenAPI spec using `npm run codegen` in the `ordning.client/` directory
- **Reusable Components**: Try to use as much reusable components as possible - check `betapet-admin/betapetadmin.client/src/components/` for examples

## Publishing

To create a release, create and push a tag starting with `v` (e.g., `v1.0.0`):
```bash
git tag v1.0.0
git push origin v1.0.0
```
