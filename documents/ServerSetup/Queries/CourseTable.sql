/* This query will create the table storing courses. */
/* All changes that are made here must also be applied in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS advising_db.courses; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS advising_db.courses
(
  course_id varchar(45) NOT NULL,
  WP int(10) unsigned NOT NULL DEFAULT 1,
  course_name varchar(45) NOT NULL,
  offered_winter tinyint(4) NOT NULL DEFAULT 0,
  offered_spring tinyint(4) NOT NULL DEFAULT 0,
  offered_summer tinyint(4) NOT NULL DEFAULT 0,
  offered_fall tinyint(4) NOT NULL DEFAULT 0,
  num_credits int(11) NOT NULL,
  department varchar(45) NOT NULL,
  num_pre_requs int(10) NOT NULL DEFAULT 0,
  PRIMARY KEY (course_id),
  UNIQUE KEY course_name_UNIQUE (course_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

