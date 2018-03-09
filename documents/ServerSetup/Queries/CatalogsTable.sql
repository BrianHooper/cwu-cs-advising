/* This query will create the table storing catalogs. */
/* All changes that are made here must also be applied in the Configuration.ini file */

 -- remove any previous table with this name
DROP TABLE IF EXISTS advising_db.catalogs; -- Note, make sure you do not delete an important table with this line!

-- Create the new table
CREATE TABLE IF NOT EXISTS advising_db.catalogs
(
  catalog_year varchar(45) NOT NULL,
  WP int(10) unsigned NOT NULL DEFAULT 1,
  first_name varchar(45) NOT NULL,
  last_name varchar(45) NOT NULL,
  starting_season varchar(45) NOT NULL,
  starting_year int(10) unsigned NOT NULL,
  expected_grad_season varchar(45) NOT NULL DEFAULT "",
  expected_grad_year int(10) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (catalog_year)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
