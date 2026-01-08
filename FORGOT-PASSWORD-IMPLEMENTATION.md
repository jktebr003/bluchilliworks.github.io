# Forgot Password Implementation

## Overview
This implementation provides a secure and user-friendly password reset functionality following industry best practices. Users can request a password reset, receive a reset token via email, and securely create a new password.

## Features Implemented

### Backend (API)

1. **Password Reset Token Fields** ([User.cs](Api/Infrastructure/Database/MongoDb/Entities/User.cs))
   - `PasswordResetToken`: Stores the 6-digit reset code
   - `PasswordResetTokenExpiry`: ISO 8601 formatted expiration timestamp (1-hour validity)

2. **Forgot Password Endpoint** ([ForgotPassword.cs](Api/Features/Users/ForgotPassword.cs))
   - **Route**: `POST /api/users/forgot-password`
   - **Security Features**:
     - Returns same response whether user exists or not (prevents email enumeration)
     - Generates secure 6-digit verification code
     - 1-hour token expiration
     - Rate limiting via API infrastructure
   - **Email Content**: Includes both code and clickable reset link

3. **Reset Password Endpoint** ([ResetPassword.cs](Api/Features/Users/ResetPassword.cs))
   - **Route**: `POST /api/users/reset-password`
   - **Validations**:
     - Email format validation
     - Token verification
     - Token expiration check
     - Strong password requirements (8+ chars, uppercase, lowercase, number, special char)
   - **Security**: Token is cleared after successful reset

### Frontend (Web App)

1. **Forgot Password Page** ([ForgotPassword.razor](Web/Features/Authentication/ForgotPassword.razor))
   - **Route**: `/authentication/forgot-password`
   - **Features**:
     - Clean, accessible UI with MudBlazor components
     - Email validation
     - Loading states with busy indicator
     - Success message display
     - Link back to login page
     - Informational guide panel

2. **Reset Password Page** ([ResetPassword.razor](Web/Features/Authentication/ResetPassword.razor))
   - **Route**: `/authentication/reset-password?email={email}&token={token}`
   - **Features**:
     - Pre-populated email and token from URL parameters
     - Real-time password strength indicator
     - Visual feedback for each password requirement
     - Password confirmation validation
     - Helpful guidance panel
     - Links to resend code or return to login

3. **State Management** (Fluxor Pattern)
   - **Actions**: `ForgotPasswordAction`, `ResetPasswordAction`
   - **Effects**: Handle async API calls with proper error handling
   - **Reducers**: Update UI state based on action results
   - **Service**: Integrated into `AuthenticationService` with API health checks

### Shared Models

- `ForgotPasswordRequest`: Email address
- `ResetPasswordRequest`: Email, reset token, new password

## Security Best Practices Implemented

### 1. **Anti-Enumeration Protection**
   - Same response returned regardless of whether email exists
   - Prevents attackers from identifying valid user accounts

### 2. **Token Security**
   - Short-lived tokens (1 hour expiration)
   - Single-use tokens (cleared after successful reset)
   - Secure random generation using `SecurityExtension.CreateRandomVerificationCode()`

### 3. **Strong Password Requirements**
   - Minimum 8 characters
   - Must contain uppercase letter
   - Must contain lowercase letter
   - Must contain number
   - Must contain special character
   - Password confirmation required

### 4. **Secure Communication**
   - Email contains both code and link for flexibility
   - Tokens transmitted via HTTPS
   - No sensitive data in email (only reset token)

### 5. **Password Hashing**
   - Uses Argon2 via `IPasswordHashingService`
   - Industry-standard password storage

## User Flow

1. **Initiate Reset**
   - User clicks "Forgot password?" on login page
   - Navigates to `/authentication/forgot-password`
   - Enters email address and submits

2. **Receive Reset Email**
   - User receives email with:
     - 6-digit reset code
     - Clickable reset link
     - 1-hour expiration notice

3. **Reset Password**
   - User clicks link or navigates to reset page manually
   - Enters or verifies email and token (pre-filled from URL)
   - Creates new password with real-time validation feedback
   - Confirms password matches

4. **Complete Reset**
   - Success message displayed
   - Automatically redirected to login page
   - Can immediately log in with new password

## Configuration Required

### API Configuration (appsettings.json)

```json
{
  "App": {
    "BaseUrl": "http://localhost:5000"
  },
  "Email": {
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": "587",
      "Username": "your-smtp-username",
      "Password": "your-smtp-password",
      "EnableSsl": "true"
    },
    "FromEmail": "noreply@bluchilliworks.com",
    "FromName": "BluChilli Works"
  }
}
```

## Testing Checklist

- [ ] User receives reset email with correct code and link
- [ ] Reset link pre-populates email and token fields
- [ ] Invalid/expired tokens are rejected with clear error
- [ ] Password validation works correctly (all requirements)
- [ ] Password confirmation validates match
- [ ] Non-existent email returns generic success (anti-enumeration)
- [ ] Token expires after 1 hour
- [ ] Token can only be used once
- [ ] Password is properly hashed in database
- [ ] User can log in with new password after reset
- [ ] UI shows appropriate loading states
- [ ] Error messages are clear and helpful

## UI/UX Features

1. **Visual Password Strength Indicator**
   - Real-time feedback as user types
   - Checkmarks turn green when requirements met
   - Clear list of all password requirements

2. **Accessibility**
   - Proper form labels
   - Validation messages
   - Keyboard navigation support
   - Screen reader friendly

3. **Mobile Responsive**
   - Uses MudBlazor grid system
   - Adapts to different screen sizes
   - Touch-friendly interface

4. **User Guidance**
   - Step-by-step instructions
   - Contextual help text
   - Clear error messages
   - Success confirmations

## Error Handling

- API connectivity issues
- Invalid email format
- Missing or invalid reset token
- Expired reset token
- Password validation failures
- Password mismatch
- Email sending failures (logged but not exposed to user)

## Future Enhancements (Optional)

1. **Rate Limiting**: Limit reset requests per email/IP address
2. **Account Lockout**: Temporary lockout after multiple failed attempts
3. **SMS/2FA Option**: Alternative verification methods
4. **Password History**: Prevent reuse of recent passwords
5. **Email Templates**: More sophisticated HTML email templates
6. **Audit Logging**: Log all password reset attempts
7. **Multi-language Support**: Internationalization
8. **CAPTCHA**: Prevent automated abuse

## Integration with Existing System

This implementation seamlessly integrates with the existing:
- MongoDB user storage
- Email service infrastructure
- Fluxor state management
- Carter API routing
- MediatR command pattern
- FluentValidation
- Mapster object mapping
- Authentication/authorization system

## Notes

- The forgot password link is already present in the Login.razor page
- Email service is already configured and operational
- Password hashing service (Argon2) is already in use
- Follows same patterns as existing SetPassword functionality
- No database migrations needed (using MongoDB's dynamic schema)
