const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const { VueLoaderPlugin } = require('vue-loader');
const libs = require('./package.json');

module.exports = (env) => {
    const isProdBuild = env && env.prod;
    const isStagingBuild = env && env.staging;

    const isDevBuild = !isProdBuild && !isStagingBuild;

    const extractCSS = new MiniCssExtractPlugin({ filename: 'vendor.css' });

    let entries = Object.keys(libs.dependencies);

    return [{
        mode: isDevBuild ? 'development' : "production",
        stats: { modules: false },
        resolve: { extensions: ['.js'] },
        entry: {
            vendor: entries
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    exclude: /node_modules/
                },
                { test: /\.css(\?|$)/, use: [MiniCssExtractPlugin.loader, 'css-loader'] },
                { test: /\.(png|woff|woff2|eot|ttf|svg)(\?|$)/, use: 'url-loader?limit=100000' }
            ]
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        optimization: isProdBuild ? {
            minimizer: [new UglifyJsPlugin()]
        } : undefined,
        plugins: [
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            }),
            extractCSS,
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new VueLoaderPlugin()
        ]
    }];
};
