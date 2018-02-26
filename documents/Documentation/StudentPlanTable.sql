/* This query will create the table storing graduation plans, and then create the master record. */
/* Replace [DB_NAME] with the name of the database (this database must already exist), e.g. advising_db */
/* Replace [STUDENT_PLANS_TABLE_NAME] with the name of the student plan table, e.g. student_plans */
/* Replace [STUDENT_PLANS_TABLE_KEY] with the name of the key for this table, e.g. SID */
/* Do not change anything that is not in square brackets [], and remove all square brackets from the query according to the instructions above */
/* All changes that are made to the above three values must also be applied to the [MySQL Connection], and [MySql Tables] groups in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS [DB_NAME].[STUDENT_PLANS_TABLE_NAME]; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS [DB_NAME].[STUDENT_PLANS_TABLE_NAME]([STUDENT_PLANS_TABLE_KEY] VARCHAR(45) NOT NULL,
WP INT(10) UNSIGNED NOT NULL DEFAULT 1, start_qtr VARCHAR(45) NULL DEFAULT "", PRIMARY KEY ([STUDENT_PLANS_TABLE_KEY]),
UNIQUE INDEX [STUDENT_PLANS_TABLE_KEY]_UNIQUE ([STUDENT_PLANS_TABLE_KEY] ASC));

-- Create master record
INSERT INTO [DB_NAME].[STUDENT_PLANS_TABLE_NAME] ([STUDENT_PLANS_TABLE_KEY], WP, start_qtr) VALUES (-1, 0, "MASTER RECORD");

/* Example: 

DROP TABLE IF EXISTS advising_db.student_plans; -- remove any previous table with this name
CREATE TABLE IF NOT EXISTS advising_db.student_plans(SID VARCHAR(45) NOT NULL, WP INT(10) UNSIGNED NOT NULL DEFAULT 1,
start_qtr VARCHAR(45) NULL DEFAULT "", PRIMARY KEY (SID, UNIQUE INDEX SID_UNIQUE (SID ASC));
INSERT INTO advising_db.student_plans (SID, WP, start_qtr) VALUES (-1, 0, "MASTER RECORD");

*/