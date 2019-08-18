const express = require('express');
const fs = require('fs');
const path = require('path');
const md5File = require('md5-file');
const recursive = require("recursive-readdir");
const config = require("./config.js");

const app = express();
let FileList = "";
const configJson = JSON.stringify(config);
const dir = path.normalize(__dirname + "/win32/");

recursive(dir, (err, files) => {
    const list = [];
    console.log("Generating file list");
    files.forEach(file => {
        list.push({
            directory: path.dirname(path.normalize(file).replace(dir, "")),
            fileName: path.basename(file),
            fileHash: md5File.sync(file),
            dlLink: config.FileCDN + path.normalize(file).replace(dir, "").replace(/\\/g,"/")
        })
    });
    console.log("Generating file done");
    FileList = JSON.stringify(list);
});


app.get('/fileList', function (req, res) {
    res.append('content-type', 'application/json');
    res.send(FileList);
});

app.get('/version', function (req, res) {
    res.append('content-type', 'application/json');
    res.send(configJson);
});

const iniContent = fs.readFileSync(__dirname + "/updater.ini");

app.get('/updater.ini', function (req, res) {
    res.append('content-type', 'text/plain');
    res.send(iniContent);
});


app.listen(3000);

console.log("Server started");