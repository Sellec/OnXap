var path = require('path');

const VueLoaderPlugin = require('vue-loader/lib/plugin');
const CopyPlugin = require('copy-webpack-plugin');
const TerserJSPlugin = require('terser-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

const isProduction = true;

module.exports = {
    mode: isProduction ? 'production' : 'development',
    entry: {
        primevue: './src/main.js'
    },
    output: {
        filename: 'primevue.js',
        path: path.resolve(__dirname, 'dist'),
        library: 'PrimeVueLibrary'
    },
    externals: {
        vue: 'Vue',
        quill: 'Quill'
    },
    optimization: {
        minimize: isProduction,
        minimizer: [new TerserJSPlugin({}), new OptimizeCSSAssetsPlugin({})],
        splitChunks: {
            cacheGroups: {
                extractedCSS: {
                    test: (module, chunks) => module.constructor.name === 'CssModule',
                    name: "primevue",
                    chunks: "all",
                    enforce: true
                }
            }
        }
    },
    resolve: {
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        },
        extensions: ['.js', '.vue', '.json']
    },
    module: {
        rules: [
            // это будет применяться к файлам `.vue`
            {
                test: /\.vue$/,
                loader: 'vue-loader'
            },
            // это будет применяться к файлам `.js`
            // А ТАКЖЕ к секциям `<script>` внутри файлов `.vue`
            {
                test: /\.m?js$/,
                exclude: /(node_modules|bower_components)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            },
            // это будет применяться к файлам `.css`
            // А ТАКЖЕ к секциям `<style>` внутри файлов `.vue`
            {
                test: /\.css$/i,
                use: [
                    //process.env.NODE_ENV !== 'production' ? 'vue-style-loader' : MiniCssExtractPlugin.loader,
                    MiniCssExtractPlugin.loader,
                    //'vue-style-loader',
                    'css-loader'
                ]
            },
            {
                test: /\.(ttf|eot|svg|woff(2)?)(\?[a-z0-9=&.]+)?$/,
                loader: 'file-loader?name=[name].[ext]&outputPath=fonts'
            },
            {
                test: /\.(jpg|png|gif)$/i,
                loader: 'file-loader?name=[name].[ext]&outputPath=images/'
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin(),
        new VueLoaderPlugin(),
        new CopyPlugin([
            'node_modules/vue/dist/vue.min.js',
            { from: 'node_modules/primevue/resources/themes/', to: 'themes/' },
            'node_modules/quill/dist/quill.min.js'
        ]),
        new MiniCssExtractPlugin({
            filename: '[name].css'
        })
    ]
};
