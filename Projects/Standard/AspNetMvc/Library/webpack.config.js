﻿const VueLoaderPlugin = require('vue-loader/lib/plugin');

module.exports = {
    entry: null,
    output: null,
    mode: 'production',
    //mode: 'development',
    //watch: true,
    externals: {
        vue: 'Vue'
        //quill: 'Quill'
    },
    resolve: {
        alias: {
            'vue$': 'vue/dist/vue.common.js'
        },
        extensions: ['.js', '.vue', '.json']
    },
    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader'
            },
            {
                test: /\.js$/,
                //exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [
                            ["@babel/preset-env", { targets: { ie: "11" } }]
                        ],
                        plugins: ['@babel/transform-object-assign', '@babel/plugin-proposal-class-properties']
                    }
                }
            },
            {
                test: /\.css$/i,
                use: [
                    'vue-style-loader',
                    'css-loader'
                ]
            }
        ]
    },
    plugins: [
        new VueLoaderPlugin()
    ],
    performance: {
        hints: false
    }
};
