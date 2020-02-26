'use strict';

var compression = require('compression');
var express = require('express');
var app = express();
app.use(compression());

var port = process.env.PORT || 1337;

app.get('/', function (req, res) {
    res.sendFile('public/index.html', { "root": __dirname });
});
app.use(express.static('public'));
app.use(express.static('dist'));

var server = app.listen(port, function () {
    console.log('Node server is running on port..' + port);
});  