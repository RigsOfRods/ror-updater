const express = require('express');
const fs = require('fs');
const path = require('path');
const md5File = require('md5-file');
var recursive = require("recursive-readdir");
var config = require("./config.js");

const app = express();
const FileList = [];

const dir = path.normalize(__dirname + "/win32/");

recursive(dir, (err, files) => {
    files.forEach(file => {
        FileList.push({
            filename: path.normalize(file).replace(dir, "").replace(/\\/g,"/"),
            hash: md5File.sync(file)
        })
    });
});


app.get('/', function (req, res) {
    res.append('content-type', 'application/json');
    res.send(JSON.stringify(FileList));
});

app.get('/version', function (req, res) {
    res.append('content-type', 'application/json');
    res.send(JSON.stringify(config.RoRversion));
});


app.listen(3000);

console.log("Server started");