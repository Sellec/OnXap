var path = require('path');

const CopyPlugin = require('copy-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    mode: 'production',
    entry: './src/standardui.js',
    output: {
        filename: 'standardui.js',
        path: path.resolve(__dirname, 'dist'),
        library: 'StandardUI'
    },
    module: {
        rules: [
            {
                test: /\.m?js$/,
                exclude: file => (
                    /node_modules/.test(file) &&
                    !/\.vue\.js/.test(file)
                ),
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            }
        ]
    },
    plugins: [
        //new CleanWebpackPlugin(),
        new CopyPlugin([
            { from: 'node_modules/vue/dist/vue.min.js', to: 'vue/' },
            { from: 'node_modules/primevuelibrary/dist', to: 'primevuelibrary/', ignore: ['**/themes/**/*'] },
            { from: 'node_modules/primevuelibrary/dist/themes/nova-accent', to: 'primevuelibrary/themes/nova-accent/' },
            { from: 'node_modules/jqueryui/jquery-ui.min.css', to: 'jqueryui/' },
            { from: 'node_modules/jqueryui/jquery-ui.min.js', to: 'jqueryui/' },
            { from: 'node_modules/jquery/dist/jquery.min.js', to: 'jquery/' }
        ])
    ]
};
