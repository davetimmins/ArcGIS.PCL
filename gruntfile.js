module.exports = function (grunt) {
    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        clean: ["dist"],
        shell: {
            makeDir: {
                command: 'mkdir dist'
            }
        },
        msbuild: {
            src: ['ArcGIS.ServiceModel\\ArcGIS.ServiceModel.csproj'],
            options: {
                projectConfiguration: 'Release',
                targets: ['Clean', 'Rebuild'],
                stdout: true
            }
        },
        assemblyinfo: {
            options: {
                files: ['ArcGIS.ServiceModel\\ArcGIS.ServiceModel.csproj'],
                info: {                    
                    title: 'ArcGIS.ServiceModel',
                }
            }
        },
        nugetpack: {
            dist: {
                src: '*.nuspec',
                dest: 'dist/',
                options: {
                    version: "3.0.0"
                }
            }
        }
    });
    grunt.loadNpmTasks('grunt-dotnet-assembly-info');
    grunt.loadNpmTasks('grunt-msbuild');
    // load contrib clean for prep
    grunt.loadNpmTasks('grunt-contrib-clean');
    // load the shell plugin for cmd goodies
    grunt.loadNpmTasks('grunt-shell');
    // Load the plugin that provides the "nuget" task.
    grunt.loadNpmTasks('grunt-nuget');
    // Default task(s).
    grunt.registerTask('default', ['clean', 'shell', 'msbuild', 'assemblyinfo', 'nugetpack']);
};
