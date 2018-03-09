/* This query will create the table storing degrees. */
/* All changes that are made here must also be applied in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS advising_db.degrees; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS advising_db.degrees 
(
  degree_id varchar(45) NOT NULL,
  WP int(10) unsigned NOT NULL DEFAULT 1,
  degree_name varchar(45) NOT NULL,
  department varchar(45) NOT NULL,
  num_requs int(10) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (degree_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
