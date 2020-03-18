# EXIF-Rewrite
Simple GUI to re-write EXIF tags in bulk from CSV

This is a (fairly basic) C# .net GUI to take a CSV of updated image locations (lat/long/height) and update all of the exif tags with these new tags.

Still in testing as bugs are found :)



## Usage

* Run the executable
* Load in the CSV of tags
* Check the assocation from CSV columns to the EXIF fields (these are dropdowns so you can change them, app tries to guess)
* Select all of the images you want to update
* Select an output folder for the processed images - this CANNOT be the source folder (so you dont overwrite your originals)
* Click start

If you miss a step, start wont enable :D

All Images must have a matching line in the CSV (but the CSV can have extras)

Altitude must be in Meters at the moment


