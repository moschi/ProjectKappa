# ProjectKappa

A tool to automate processing *.las* files



**Description of program flow:**

---

1. las2las: create a tree of all directories containing .las files starting from a defined root directory
   *for each directory, refered to in the following by ``{dirnr}``*
   1. compile a list of *PAN.las files to convert to .laz
      1. command will be in the following format:  ```“las2las -lof file_list.{dirnr}.txt -keep_classification 2 -target_sp83 PA_N -odir "E:\Pennsylvania Iron and Charcoal\LAS Files\{dirnr}\las2las" -olaz”```
      2. compile a list of *PAS.las files to convert to .laz
         1. command will be in the following format:  ``“las2las -lof file_list.{dirnr}.txt -keep_classification 2 -target_sp83 PA_S -odir "E:\Pennsylvania Iron and Charcoal\LAS Files\{dirnr}\las2las" -olaz”``

*note: every directory (e.g. 26) will further contain a las2las sub-directory in which the desired output in the .laz format will be created*

---

2. lastile: create a tree of all directories containing .laz files starting from a defined root directory 
   *for each directory, refered to in the following by ``{dirnr}``*
   1. compile a list of *.laz files to convert to tile_*.laz files 
      1. command will be in the following format: ``“lastile -lof file_list.{dirnr}.txt -o "tile.laz" -tile_size 1000 -buffer 25 -odir "E:\Pennsylvania Iron and Charcoal\LAS Files\{dirnr}\lastile" -olaz”``

*note: every directory (e.g. 26) will further contain a lastile sub-directory in which the desired output in the .laz format will be created*

---

3. BLAST2DEM: create a tree of all directories containing tile_*.laz files starting from a defined root directory
   *for each directory, refered to in the following by ``{dirnr}``*
   1. compile a list of *.laz files to convert to *.tif, *.kml, and *.tfw
      1. command will be in the following format: ``“blast2dem -lof file_list.{dirnr}.txt -elevation -odir "E:\Pennsylvania Iron and Charcoal\LAS Files\{dirnr}\blast2dem" -otif”``

*note: every directory (e.g. 26) will further contain a blast2dem sub-directory in which the desired output will be created*

---

