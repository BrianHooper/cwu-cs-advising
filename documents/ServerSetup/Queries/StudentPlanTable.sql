/* This query will create the table storing graduation plans, and then create the master record. */
/* All changes that are made here must also be applied in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS advising_db.student_plans; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS advising_db.student_plans
(
  SID VARCHAR(45) NOT NULL,
  WP INT(10) UNSIGNED NOT NULL DEFAULT 1,
  start_qtr VARCHAR(45) NULL DEFAULT "",
  PRIMARY KEY (SID),
  UNIQUE INDEX SID_UNIQUE (SID ASC)
);

-- Create master record
INSERT INTO advising_db.student_plans (SID, WP, start_qtr) VALUES ("-1", 0, "MASTER RECORD");
