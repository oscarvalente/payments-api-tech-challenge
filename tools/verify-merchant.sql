UPDATE payments_api.merchants
SET is_verified = true
WHERE username = @username;