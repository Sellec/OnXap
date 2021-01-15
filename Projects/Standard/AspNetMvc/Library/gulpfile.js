/// <binding BeforeBuild='build, copyResources' ProjectOpened='default' />
const gulp = require('gulp');
const gutil = require('gulp-util');
var minify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');
const fs = require('fs');
const path = require('path');
const browserify = require('browserify');
const watchify = require('watchify');
const fsPath = require('fs-path');
const babelify = require('babelify');

var source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');
var es2015 = require('babel-preset-es2015');

function getTargets() {
    var dir = "Modules";
    var sourceJsFolder = "SourceJs";
    var results = new Array();
    fs.
        readdirSync(dir).
        filter(file => fs.statSync(path.join(dir, file)).isDirectory() && fs.existsSync(path.join(dir, file, sourceJsFolder)) && fs.statSync(path.join(dir, file, sourceJsFolder)).isDirectory()).
        map(function (moduleFolder) {
            var moduleSourceFolderPath = path.join(dir, moduleFolder, sourceJsFolder);
            fs.
                readdirSync(moduleSourceFolderPath).
                filter(file => file.endsWith(".js") && fs.statSync(path.join(moduleSourceFolderPath, file)).isFile()).
                map(function (sourceJsFile) {
                    results[results.length] = {
                        input: path.join(moduleSourceFolderPath, sourceJsFile),
                        outputFolder: path.join("Data//Modules", moduleFolder),
                        outputFileName: sourceJsFile,
                        standalone: sourceJsFile.replace(".js", "").replace(".", "_")
                    };
                });
        });
    return results;
}

const paths = [];

function watchFolder(target) {
    var b = browserify({
        entries: [target.input],
        cache: {},
        packageCache: {},
        plugin: [watchify],
        basedir: process.env.INIT_CWD
    });

    function bundle() { 
        b.bundle();
        compileJS(target);
        gutil.log("Bundle rebuil1t!");
    }
    b.on('update', bundle);
    bundle();
}

function compileJS(target) {
    // set up the browserify instance on a task basis
    browserify({
        debug: true,
        entries: [target.input],
        basedir: process.env.INIT_CWD,
        standalone: target.standalone
    }).
        transform(babelify, { presets: ["es2015"] }).
        bundle().
        pipe(source(target.outputFileName)).
        pipe(buffer()).
        pipe(sourcemaps.init({ loadMaps: true })).
        pipe(minify()).
        on('error', gutil.log).
        //.pipe(sourcemaps.write('./'))
        pipe(gulp.dest(target.outputFolder));
}

gulp.task('build', function () {
    var targets = getTargets();
    gutil.log(targets);
    targets.map(function (target) {
        compileJS(target);
    });
});

gulp.task('default', function () {
    var targets = getTargets();
    gutil.log(targets);
    targets.map(function (target) {
        watchFolder(target);
    });
});

gulp.task('copyResources', function () {
    gulp.src('node_modules/vue/dist/vue.min.js').pipe(gulp.dest('Design/UI/vue'));
    gulp.src(['node_modules/primevuelibrary/dist/**/*', '!**/themes/**/*']).pipe(gulp.dest('Design/UI/primevuelibrary'));
    gulp.src('node_modules/primevuelibrary/dist/themes/nova-accent/**/*').pipe(gulp.dest('Design/UI/primevuelibrary/themes/nova-accent'));
    gulp.src('node_modules/jqueryui/jquery-ui.min.css').pipe(gulp.dest('Design/UI/jqueryui'));
    gulp.src('node_modules/jqueryui/jquery-ui.min.js').pipe(gulp.dest('Design/UI/jqueryui'));
    gulp.src('node_modules/jquery/dist/jquery.min.js').pipe(gulp.dest('Design/UI/jquery'));
});
