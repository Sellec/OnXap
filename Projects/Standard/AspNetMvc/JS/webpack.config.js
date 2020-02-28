var path = require('path');

const CopyPlugin = require('copy-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    mode: 'production',
    entry: './src/main.js',
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'dist')
    },
    plugins: [
        new CleanWebpackPlugin(),
        new CopyPlugin([
            { from: 'node_modules/vue/dist/vue.min.js', to: 'vue/' },
            { from: 'node_modules/primevuelibrary/dist', to: 'primevuelibrary/', ignore: ['**/themes/**/*'] },
            { from: 'node_modules/primevuelibrary/dist/themes/nova-light', to: 'primevuelibrary/themes/nova-light/' },
            { from: 'node_modules/jqueryui/jquery-ui.min.css', to: 'jqueryui/' },
            { from: 'node_modules/jqueryui/jquery-ui.min.js', to: 'jqueryui/' },
            { from: 'node_modules/jquery/dist/jquery.min.js', to: 'jquery/' }
        ])
    ]
};
