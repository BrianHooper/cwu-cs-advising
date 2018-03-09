/* This query will create the table storing user credentials. */
/* All changes that are made here must also be applied in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS advising_db.user_credentials; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS advising_db.user_credentials
(
  username varchar(45) NOT NULL,
  WP int(10) unsigned NOT NULL DEFAULT 1,
  password char(128) NOT NULL,
  admin tinyint(4) NOT NULL DEFAULT 0,
  password_salt binary(32) NOT NULL,
  active tinyint(4) NOT NULL DEFAULT 0,
  PRIMARY KEY (username),
  UNIQUE KEY username_UNIQUE (username),
  UNIQUE KEY password_salt_UNIQUE (password_salt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
