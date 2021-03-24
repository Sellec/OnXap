/// <binding BeforeBuild='copyResources' ProjectOpened='default' />
const gulp = require('gulp');
const gutil = require('gulp-util');
const connect = require('gulp-connect');

const webpack = require('webpack');
const webpackStream = require('webpack-stream');

const sourcemaps = require('gulp-sourcemaps');
const fs = require('fs');
const path = require('path');

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
                        output: {
                            directory: path.join("Data//Modules", moduleFolder),
                            filename: sourceJsFile,
                            standalone: sourceJsFile.replace(".js", "").replace(".", "_")
                        }
                    };
                });
        });
    return results;
}

function compileJS(target) {
    var webpackConfig = require('./webpack.config.js');
    webpackConfig.entry = ['./' + target.input];
    webpackConfig.output = {
        path: '/' + target.output.directory,
        filename: target.output.filename,
        library: target.output.standalone
    };
    //webpackConfig.mode = 'development';
    return gulp.src(target.input)
        .pipe(webpackStream(webpackConfig, webpack))
        .pipe(sourcemaps.init({ loadMaps: true }))
        .on('error', gutil.log)
        //.pipe(sourcemaps.write('./'))
        .pipe(gulp.dest(target.output.directory))
        .pipe(connect.reload());
}

gulp.task('build', function () {
    var targets = getTargets();
    targets.map(function (target) {
        compileJS(target);
    });
});

gulp.task('copyResources', function () {
    gulp.src('node_modules/vue/dist/vue.min.js').pipe(gulp.dest('Design/UI/vue'));

    gulp.src(['node_modules/primevue/resources/**/*', '!**/themes/**/*', '!**/primevue.css']).pipe(gulp.dest('Design/UI/primevue'));
    gulp.src('node_modules/primevue/resources/themes/nova-accent/**/*').pipe(gulp.dest('Design/UI/primevue/themes/nova-accent'));

    gulp.src('node_modules/primeicons/fonts/**/*').pipe(gulp.dest('Design/UI/primeicons/fonts'));
    gulp.src('node_modules/primeicons/primeicons.css').pipe(gulp.dest('Design/UI/primeicons'));

    gulp.src('node_modules/jqueryui/jquery-ui.min.css').pipe(gulp.dest('Design/UI/jqueryui'));
    gulp.src('node_modules/jqueryui/jquery-ui.min.js').pipe(gulp.dest('Design/UI/jqueryui'));
    gulp.src('node_modules/jquery/dist/jquery.min.js').pipe(gulp.dest('Design/UI/jquery'));

    gulp.src('node_modules/@babel/polyfill/dist/polyfill.min.js').pipe(gulp.dest('Design/UI/tools'));
});

// Default task
gulp.task('default', ['build', 'copyResources']);

