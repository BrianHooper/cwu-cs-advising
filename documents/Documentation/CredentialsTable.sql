/* This query will create the table storing user credentials. */
/* Replace [DB_NAME] with the name of the database (this database must already exist), e.g. advising_db */
/* Replace [CREDENTIALS_TABLE_NAME] with the name of the student plan table, e.g. user_credentials */
/* Replace [CREDENTIALS_TABLE_KEY] with the name of the key for this table, e.g. username */
/* Do not change anything that is not in square brackets [], and remove all square brackets from the query according to the instructions above */
/* All changes that are made to the above three values must also be applied to the [MySQL Connection], and [MySql Tables] groups in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS [DB_NAME].[CREDENTIALS_TABLE_NAME]; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS [DB_NAME].[CREDENTIALS_TABLE_NAME] ([CREDENTIALS_TABLE_KEY] VARCHAR(45) NOT NULL, WP INT(10) UNSIGNED NOT NULL DEFAULT 1,
password CHAR(64) NOT NULL, admin TINYINT(4) NOT NULL DEFAULT 0, password_salt BINARY(32) NOT NULL, active TINYINT(4) NOT NULL DEFAULT 0,
PRIMARY KEY ([CREDENTIALS_TABLE_KEY]), UNIQUE INDEX [CREDENTIALS_TABLE_KEY]_UNIQUE ([CREDENTIALS_TABLE_KEY] ASC),
UNIQUE INDEX password_salt_UNIQUE (password_salt ASC));

/* Example: 

CREATE TABLE IF NOT EXISTS advising_db.user_credentials (username VARCHAR(45) NOT NULL, WP INT(10) UNSIGNED NOT NULL DEFAULT 1,
password CHAR(64) NOT NULL, admin TINYINT(4) NOT NULL DEFAULT 0, password_salt BINARY(32) NOT NULL, active TINYINT(4) NOT NULL DEFAULT 0,
PRIMARY KEY (username), UNIQUE INDEX username_UNIQUE (username ASC), UNIQUE INDEX password_salt_UNIQUE (password_salt ASC));

*/