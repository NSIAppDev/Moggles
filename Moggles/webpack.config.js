const path = require('path');
const webpack = require('webpack');
const nodeExternals = require('webpack-node-externals');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const { VueLoaderPlugin } = require('vue-loader');
const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isProdBuild = env && env.prod;
    const isStagingBuild = env && env.staging;
    const isTesting = env === "test";
    const isDevBuild = !isProdBuild && !isStagingBuild && !isTesting;

    let devPlugins = [
        // Plugins that apply in development builds only
        new webpack.SourceMapDevToolPlugin({
            filename: '[file].map', // Remove this line if you prefer inline source maps
            moduleFilenameTemplate:
                path.relative(bundleOutputDir,
                    '[resourcePath]') // Point sourcemap entries to the original file locations on disk
        })
    ];

    let stagingPlugins = [
        // Plugins that apply in development builds only
        new webpack.SourceMapDevToolPlugin({
            filename: '[file].map', // Remove this line if you prefer inline source maps
            moduleFilenameTemplate:
                path.relative(bundleOutputDir,
                    '[resourcePath]') // Point sourcemap entries to the original file locations on disk
        })
    ];

    let minimizerUglify =
        new UglifyJsPlugin({
            sourceMap: true,
            uglifyOptions: {
                warnings: false
            }
        });

    return {
        mode: isDevBuild ? 'development' : "production",
        entry: { 'main': ['./ClientApp/src/boot.js', './sass/moggles.scss'] },
        context: __dirname,
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
        //node: {
        //    fs: 'empty'
        //},
        module: {
            rules: [
                {
                    enforce: 'pre',
                    test: /\.(js|vue)$/,
                    loader: 'eslint-loader',
                    exclude: /node_modules/
                },
                {
                    test: /\.css$/,
                    use: ['vue-style-loader', 'css-loader']
                },
                {
                    test: /\.scss$/,
                    use: [
                        {
                            loader: 'file-loader',
                            options: {
                                name: '[name].css',
                                outputPath: '../css/'
                            }
                        },
                        {
                            loader: 'extract-loader'
                        },
                        {
                            loader: 'css-loader'
                        },
                        {
                            loader: 'postcss-loader'
                        },
                        {
                            loader: 'sass-loader'
                        }
                    ]
                },
                {
                    test: /\.sass$/,
                    use: [
                        'vue-style-loader',
                        'css-loader',
                        'sass-loader?indentedSyntax'
                    ]
                },
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    options: {
                        loaders: {
                            css: ['vue-style-loader', 'css-loader'],
                            'scss': ['vue-style-loader', 'css-loader', 'sass-loader'],
                            'sass': ['vue-style-loader', 'css-loader', 'sass-loader?indentedSyntax']
                        },
                        extractCSS: isDevBuild ? false : true,
                        optimizeSSR: false
                    },
                    exclude: /node_modules/
                },
                {
                    test: /\.js$/,
                    loader: 'babel-loader',
                    exclude: /node_modules/
                },
                {
                    test: /\.(png|woff|woff2|eot|ttf|svg)(\?|$)/,
                    loader: 'url-loader',
                    options: {
                        name: '[name].[ext]?[hash]',
                        outputPath: 'assets/',
                        limit: 8192
                    }
                }
            ]
        },
        resolve: {
            alias: {
                'vue$': 'vue/dist/vue.esm.js'
            },
            extensions: ['*', '.js', '.vue', '.json']
        },
        target: isTesting ? 'node' : 'web',
        externals: isTesting ? [nodeExternals()] : [],
        devtool: isTesting ? 'inline-cheap-module-source-map' : undefined,
        optimization: isProdBuild ? {
            minimizer: [minimizerUglify]
        } : undefined,
        plugins: [
            new webpack.DefinePlugin({
                'process.env': {
                    NODE_ENV: JSON.stringify(isDevBuild ? 'development' : 'production')
                }
            }),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            new VueLoaderPlugin()
        ].concat(isDevBuild ? devPlugins : [])
            .concat(isStagingBuild ? stagingPlugins : [])
    }
};