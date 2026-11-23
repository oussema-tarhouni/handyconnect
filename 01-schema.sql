-- HandyConnect — MySQL 8 schema
-- Run order: 01-schema.sql, then 02-reference-data.sql.
-- (Docker Compose does this automatically on first start.)

CREATE DATABASE IF NOT EXISTS handyconnect
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE handyconnect;

CREATE TABLE governorates (
  id      TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name_fr VARCHAR(50) NOT NULL,
  name_ar VARCHAR(50) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_governorates_name_fr (name_fr)
) ENGINE=InnoDB;

CREATE TABLE trades (
  id      TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
  slug    VARCHAR(30) NOT NULL,
  name_fr VARCHAR(50) NOT NULL,
  name_ar VARCHAR(50) NOT NULL,
  icon    VARCHAR(20) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_trades_slug (slug)
) ENGINE=InnoDB;

CREATE TABLE users (
  id             INT UNSIGNED NOT NULL AUTO_INCREMENT,
  full_name      VARCHAR(100) NOT NULL,
  email          VARCHAR(150) NULL,
  phone          VARCHAR(20)  NOT NULL,            -- normalised, e.g. +21620123456
  password_hash  VARCHAR(100) NOT NULL,            -- bcrypt
  role           ENUM('client','artisan','admin') NOT NULL DEFAULT 'client',
  governorate_id TINYINT UNSIGNED NULL,
  created_at     DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (id),
  UNIQUE KEY uq_users_email (email),               -- several NULLs are allowed
  UNIQUE KEY uq_users_phone (phone),
  CONSTRAINT fk_users_governorate FOREIGN KEY (governorate_id) REFERENCES governorates (id)
) ENGINE=InnoDB;

-- One row per artisan account. A profile is public only while
-- subscription_ends_at is in the future.
CREATE TABLE artisan_profiles (
  user_id              INT UNSIGNED NOT NULL,
  trade_id             TINYINT UNSIGNED NOT NULL,
  bio                  VARCHAR(500) NOT NULL DEFAULT '',
  zone                 VARCHAR(150) NOT NULL DEFAULT '',
  experience_years     TINYINT UNSIGNED NOT NULL DEFAULT 0,
  subscription_ends_at DATETIME NOT NULL,
  created_at           DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (user_id),
  KEY ix_artisan_profiles_trade (trade_id),
  KEY ix_artisan_profiles_subscription (subscription_ends_at),
  CONSTRAINT fk_artisan_profiles_user  FOREIGN KEY (user_id)  REFERENCES users (id) ON DELETE CASCADE,
  CONSTRAINT fk_artisan_profiles_trade FOREIGN KEY (trade_id) REFERENCES trades (id)
) ENGINE=InnoDB;

CREATE TABLE artisan_specialties (
  id         INT UNSIGNED NOT NULL AUTO_INCREMENT,
  artisan_id INT UNSIGNED NOT NULL,
  label      VARCHAR(60)  NOT NULL,
  PRIMARY KEY (id),
  KEY ix_artisan_specialties_artisan (artisan_id),
  CONSTRAINT fk_artisan_specialties_artisan FOREIGN KEY (artisan_id)
    REFERENCES artisan_profiles (user_id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Logged the first time a signed-in user reveals an artisan's phone number.
-- A client may only review an artisan they have contacted.
CREATE TABLE contact_logs (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  client_id  INT UNSIGNED NOT NULL,
  artisan_id INT UNSIGNED NOT NULL,
  created_at DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (id),
  UNIQUE KEY uq_contact_logs_pair (client_id, artisan_id),
  CONSTRAINT fk_contact_logs_client  FOREIGN KEY (client_id)  REFERENCES users (id) ON DELETE CASCADE,
  CONSTRAINT fk_contact_logs_artisan FOREIGN KEY (artisan_id) REFERENCES artisan_profiles (user_id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE reviews (
  id         INT UNSIGNED NOT NULL AUTO_INCREMENT,
  artisan_id INT UNSIGNED NOT NULL,
  client_id  INT UNSIGNED NOT NULL,
  rating     TINYINT UNSIGNED NOT NULL,
  comment    VARCHAR(500) NOT NULL DEFAULT '',
  status     ENUM('pending','approved','rejected') NOT NULL DEFAULT 'pending',
  created_at DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (id),
  UNIQUE KEY uq_reviews_artisan_client (artisan_id, client_id),
  KEY ix_reviews_artisan_status (artisan_id, status),
  CONSTRAINT ck_reviews_rating CHECK (rating BETWEEN 1 AND 5),
  CONSTRAINT fk_reviews_artisan FOREIGN KEY (artisan_id) REFERENCES artisan_profiles (user_id) ON DELETE CASCADE,
  CONSTRAINT fk_reviews_client  FOREIGN KEY (client_id)  REFERENCES users (id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE contact_messages (
  id         INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name       VARCHAR(100) NOT NULL,
  email      VARCHAR(150) NOT NULL,
  subject    VARCHAR(150) NOT NULL,
  message    TEXT NOT NULL,
  created_at DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE newsletter_subscribers (
  id         INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name       VARCHAR(100) NOT NULL,
  email      VARCHAR(150) NOT NULL,
  created_at DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
  PRIMARY KEY (id),
  UNIQUE KEY uq_newsletter_email (email)
) ENGINE=InnoDB;

-- Single source of truth for FAQ: shown on the site AND used as the
-- knowledge base of the AI assistant (and its offline fallback).
CREATE TABLE faq_entries (
  id          INT UNSIGNED NOT NULL AUTO_INCREMENT,
  sort_order  SMALLINT NOT NULL,
  question_fr VARCHAR(200) NOT NULL,
  answer_fr   VARCHAR(600) NOT NULL,
  question_ar VARCHAR(200) NOT NULL,
  answer_ar   VARCHAR(600) NOT NULL,
  keywords    VARCHAR(300) NOT NULL,   -- space separated, accent-free FR + AR
  PRIMARY KEY (id)
) ENGINE=InnoDB;
