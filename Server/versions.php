<?php
header("content-type: application/json; charset: utf-8");
$versions = array(
    "Updater" => "1.1.0.0",
    "Updater-DL" => "https://rink.hockeyapp.net/apps/b1a60e07ff0e462a927012fdb07f1c72",
    "Rigs-of-Rods" => implode(".",GetFileVersion("./win32/RoR.exe")),
    "Rigs-of-Rods-Dev" => implode(".",GetFileVersion("./win32-dev/RoR.exe")),
    );

echo json_encode($versions, JSON_PRETTY_PRINT);

function GetFileVersion($FileName) {

$handle=fopen($FileName,'rb');
if (!$handle) return FALSE;
$Header=fread ($handle,64);
if (substr($Header,0,2)!='MZ') return FALSE;
$PEOffset=unpack("V",substr($Header,60,4));
if ($PEOffset[1]<64) return FALSE;
fseek($handle,$PEOffset[1],SEEK_SET);
$Header=fread ($handle,24);
if (substr($Header,0,2)!='PE') return FALSE;
$Machine=unpack("v",substr($Header,4,2));
if ($Machine[1]!=332) return FALSE;
$NoSections=unpack("v",substr($Header,6,2));
$OptHdrSize=unpack("v",substr($Header,20,2));
fseek($handle,$OptHdrSize[1],SEEK_CUR);
$ResFound=FALSE;
for ($x=0;$x<$NoSections[1];$x++) {      //$x fixed here
    $SecHdr=fread($handle,40);
    if (substr($SecHdr,0,5)=='.rsrc') {         //resource section
        $ResFound=TRUE;
        break;
    }
}
if (!$ResFound) return FALSE;
$InfoVirt=unpack("V",substr($SecHdr,12,4));
$InfoSize=unpack("V",substr($SecHdr,16,4));
$InfoOff=unpack("V",substr($SecHdr,20,4));
fseek($handle,$InfoOff[1],SEEK_SET);
$Info=fread($handle,$InfoSize[1]);
$NumDirs=unpack("v",substr($Info,14,2));
$InfoFound=FALSE;
for ($x=0;$x<$NumDirs[1];$x++) {
    $Type=unpack("V",substr($Info,($x*8)+16,4));
    if($Type[1]==16) {             //FILEINFO resource
        $InfoFound=TRUE;
        $SubOff=unpack("V",substr($Info,($x*8)+20,4));
        break;
    }
}
if (!$InfoFound) return FALSE;
$SubOff[1]&=0x7fffffff;
$InfoOff=unpack("V",substr($Info,$SubOff[1]+20,4)); //offset of first FILEINFO
$InfoOff[1]&=0x7fffffff;
$InfoOff=unpack("V",substr($Info,$InfoOff[1]+20,4));    //offset to data
$DataOff=unpack("V",substr($Info,$InfoOff[1],4));
$DataSize=unpack("V",substr($Info,$InfoOff[1]+4,4));
$CodePage=unpack("V",substr($Info,$InfoOff[1]+8,4));
$DataOff[1]-=$InfoVirt[1];
$Version=unpack("v4",substr($Info,$DataOff[1]+48,8));
$x=$Version[2];
$Version[2]=$Version[1];
$Version[1]=$x;
$x=$Version[4];
$Version[4]=$Version[3];
$Version[3]=$x;
return $Version;
}