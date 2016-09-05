<?php

header("content-type: application/json; charset: utf-8");


$i = 0;
if(isset($_GET["DevBuilds"]))
    $devbuild = $_GET["DevBuilds"]  === 'True'? true: false;
else
    $devbuild = false;
    

if($devbuild)
    $dir = new RecursiveDirectoryIterator("win32-dev/");
else
    $dir = new RecursiveDirectoryIterator("win32/");

foreach (new RecursiveIteratorIterator($dir) as $filename) {
    
    if(basename($filename) == "." || basename($filename) == ".." ){
    }
    else{
        $dirn = dirname($filename);
        if($devbuild)
           $dirn = str_replace("win32-dev",".", $dirn);
        else
            $dirn = str_replace("win32",".", $dirn);
        $json_array[$i] = new RoRUpdaterItem($i,$dirn,basename($filename), md5_file($filename));
        $i++;
    }
}

echo json_encode(array_values($json_array), JSON_PRETTY_PRINT) ;




class RoRUpdaterItem {
    var $id;
    var $directory;
    var $fileName;
    var $fileHash;
    
    function RoRUpdaterItem($id,$directory,$fileName,$fileHash){
        $this->id = $id;
        $this->directory = $directory;
        $this->fileName = $fileName;
        $this->fileHash = $fileHash;
    }
} 