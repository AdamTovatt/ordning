# Frontend + Backend Integration Setup Checklist

This checklist helps set up a new C# backend + TypeScript React frontend project to work seamlessly when starting from Visual Studio, similar to the betapet-admin project structure.

## Backend Configuration

### 1. Project File (.csproj)
- [ ] Add SPA configuration:
  ```xml
  <SpaRoot>..\your-client-folder</SpaRoot>
  <SpaProxyLaunchCommand>npm run dev</SpaProxyLaunchCommand>
  <SpaProxyServerUrl>https://localhost:YOUR_VITE_PORT</SpaProxyServerUrl>
  ```
- [ ] Add `Microsoft.AspNetCore.SpaProxy` package reference (Version 8.*-*)
- [ ] Add project reference to client .esproj file:
  ```xml
  <ProjectReference Include="..\your-client\your-client.esproj">
    <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
  </ProjectReference>
  ```

### 2. Program.cs
- [ ] Add `app.UseDefaultFiles()` before `app.UseStaticFiles()`
- [ ] Add `app.UseRouting()` after `app.UseHttpsRedirection()`
- [ ] Add `app.MapFallbackToFile("/index.html")` before `app.Run()`

### 3. launchSettings.json
- [ ] Keep only the `https` profile (remove `http` and `IIS Express` profiles)
- [ ] Set `launchUrl` to point to Vite dev server: `"https://localhost:YOUR_VITE_PORT/"`
- [ ] Add `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES: "Microsoft.AspNetCore.SpaProxy"` to environment variables
- [ ] Create `launchSettings_git.json` template (without secrets) for git
- [ ] Add `launchSettings.json` to `.gitignore`

## Frontend Configuration

### 4. vite.config.ts
- [ ] Add `/api/*` proxy rule to proxy API calls to backend:
  ```typescript
  proxy: {
    '^/api/*': {
      target,
      secure: false
    }
  }
  ```
- [ ] Add `open: false` to server config to prevent Vite from opening browser
- [ ] Ensure certificate setup matches backend (for HTTPS dev server)

### 5. package.json (for codegen)
- [ ] Add `openapi-fetch` as dependency
- [ ] Add `openapi-typescript` as devDependency
- [ ] Add codegen script pointing to HTTP port:
  ```json
  "codegen": "openapi-typescript http://localhost:YOUR_HTTP_PORT/swagger/v1/swagger.json -o src/types/api.ts"
  ```
  **Note:** Use HTTP port (not HTTPS) to avoid certificate issues

## Authentication Setup (if needed)

### 6. Backend Auth Domain
- [ ] Create `Auth/` folder structure:
  - `User.cs` - User model
  - `IUserService.cs` - Interface for credential validation
  - `UserService.cs` - Implementation
  - `AuthRequestValidationService.cs` - Validates login and creates JWT tokens
- [ ] Add EasyReasy packages:
  - `EasyReasy` (1.0.2)
  - `EasyReasy.Auth` (1.3.1)
  - `EasyReasy.EnvironmentVariables` (1.2.2)
- [ ] Create `EnvironmentVariables.cs` with `JwtSecret` variable
- [ ] Configure in `Program.cs`:
  - Validate environment variables
  - Register EasyReasy.Auth with JWT secret
  - Register auth services
  - Add auth endpoints: `app.AddAuthEndpoints(...)`
  - Use middleware: `app.UseEasyReasyAuth()`
- [ ] Add `JWT_SECRET` to `launchSettings.json` environment variables

### 7. Frontend Auth (after codegen)
- [ ] Run `npm run codegen` to generate types
- [ ] Create `src/services/apiClient.ts` with typed client
- [ ] Create `src/services/authService.ts` for login/logout
- [ ] Create `src/contexts/AuthContext.tsx` for auth state
- [ ] Create `src/pages/LoginPage.tsx` for login UI
- [ ] Create `src/components/ProtectedRoute.tsx` for route protection
- [ ] Wrap app with `AuthProvider` in `App.tsx`

## Testing

### 8. Verify Setup
- [ ] Start backend from Visual Studio
- [ ] Verify only one browser window opens (frontend at Vite dev server)
- [ ] Verify frontend loads correctly
- [ ] Verify API calls proxy to backend (check network tab)
- [ ] Verify Swagger is accessible at `/swagger`
- [ ] Run `npm run codegen` successfully
- [ ] Test login flow (if auth is set up)

## Common Issues & Solutions

### Two browser windows open
- Check `launchSettings.json` - should only have `https` profile with `launchBrowser: true`
- Check `vite.config.ts` - should have `open: false`

### Proxy errors (ECONNREFUSED)
- Verify backend is running
- Check proxy target URL matches backend port
- Try using `127.0.0.1` instead of `localhost` if IPv6 issues occur

### Codegen certificate errors
- Use HTTP port instead of HTTPS in codegen script
- Or set `NODE_TLS_REJECT_UNAUTHORIZED=0` (not recommended for production)

### Frontend doesn't load
- Verify Vite dev server port matches `SpaProxyServerUrl` in .csproj
- Verify `launchUrl` in launchSettings.json points to Vite dev server
- Check browser console for errors

## Notes

- The Vite dev server port should be unique per project (e.g., 55903, 55937)
- The backend HTTP port is typically 5196 (or similar)
- The backend HTTPS port is typically 7152 (or similar)
- Always use HTTP port for codegen to avoid certificate issues
- Keep `launchSettings.json` out of git, use `launchSettings_git.json` as template
