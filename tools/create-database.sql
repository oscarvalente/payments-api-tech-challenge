START TRANSACTION;
CREATE DATABASE IF NOT EXISTS payments_api;
USE payments_api;
CREATE TABLE IF NOT EXISTS merchants(
    id INT AUTO_INCREMENT,
    username VARCHAR(20) UNIQUE NOT NULL,
    address VARCHAR(90) NOT NULL,
    password_hash VARCHAR(88) NOT NULL,
    password_salt VARCHAR(383) NOT NULL,
    is_verified BOOL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
);
CREATE TABLE IF NOT EXISTS payments(
    id INT AUTO_INCREMENT,
    merchant_id INT,
    ref_uuid VARCHAR(36) NOT NULL UNIQUE,
    amount DECIMAL(5, 2) NOT NULL,
    currency_code VARCHAR(3) NOT NULL, -- ISO 4217
    card_holder VARCHAR(30) NOT NULL, -- TODO: should be encrypted
    pan VARCHAR(16) NOT NULL, -- TODO: should be encrypted
    expiry_date DATE NOT NULL,
    acquiring_bank_swift VARCHAR(11) NOT NULL,
    is_accepted BOOL NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    FOREIGN KEY (merchant_id) REFERENCES merchants(id) ON UPDATE RESTRICT ON DELETE NO ACTION
);
COMMIT;