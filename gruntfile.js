'use strict';

module.exports = function (grunt) {

    require('load-grunt-tasks')(grunt);
    require('time-grunt')(grunt);

    grunt.initConfig({
        clean: ["dist"],
        shell: {
            makeDir: {
                command: 'mkdir dist'
            }
        },
        msbuild: {
            src: ['*.sln'],
            options: {
                projectConfiguration: 'Release',
                targets: ['Clean', 'Rebuild'],
                stdout: true
            }
        },
        xunit: {
            tests: {
                src: "ArcGIS.Test/bin/Release/ArcGIS.Test.dll"
            }
        },
        nugetpack: {
            dist: {
                src: '*.nuspec',
                dest: 'dist/'
            }
        }
    });

    grunt.registerTask('build', ['clean', 'shell', 'msbuild']);
    grunt.registerTask('default', ['build', 'xunit', 'nugetpack']);
    grunt.registerTask('dirty', ['build', 'nugetpack']);
};
