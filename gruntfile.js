module.exports = function (grunt) {
    "use strict";

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
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
        cs_xunit: {
            options: {
                xUnit: "xunit\\xunit.console.exe"
            },
            data: {
                dll: "ArcGIS.Test\\bin\\Release\\ArcGIS.Test.dll"
            }
        },
        nugetpack: {
            dist: {
                src: '*.nuspec',
                dest: 'dist/'
            }
        }
    });
   
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-shell');
    grunt.loadNpmTasks('grunt-msbuild');
    grunt.loadNpmTasks('grunt-cs-xunit');
    grunt.loadNpmTasks('grunt-nuget');
    
    grunt.registerTask('default', ['clean', 'shell', 'msbuild', 'cs_xunit', 'nugetpack']);
};
