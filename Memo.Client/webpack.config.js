const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = (_, args) => {
    const isProd = args.mode === 'production';
    const buildPath = './public';
    const babelConfig = {
        presets: [
            ['@babel/preset-env', {
                modules: false
            }]
        ],
        plugins: [
            'emotion'
        ],
    };

    return {
        devtool: isProd ? false : 'eval',
        entry: './src/Memo.Client.fsproj',
        output: {
            path: path.join(__dirname, buildPath),
            filename: isProd ? '[name].[contenthash].js' : '[name].js',
        },
        devServer: {
            publicPath: '/',
            contentBase: buildPath,
            port: 8080,
            historyApiFallback: true,
            proxy: {
                '/api': 'http://localhost:5000'
            }
        },
        module: {
            rules: [
                {
                    test: /\.fs(x|proj)?$/,
                    exclude: /node_modules/,
                    use: [
                        {
                            loader: 'linaria/loader'
                        },
                        {
                            loader: 'fable-loader',
                            options: {
                                babel: babelConfig,
                            },
                        }
                    ]
                },
                {
                    test: /\.(css|scss)$/,
                    use: [
                        {
                            loader: MiniCssExtractPlugin.loader,
                            options: {
                                hmr: !isProd,
                            },
                        },
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: !isProd,
                            },
                        },
                        { loader: 'sass-loader' }
                    ],
                },
            ]
        },
        plugins: [
            new CleanWebpackPlugin(),
            new MiniCssExtractPlugin({
                filename: 'style.[contenthash].css'
            }),
            new HtmlWebpackPlugin({
                filename: 'index.html',
                template: './src/index.html',
                cache: false // Prevents index.html clean (caused by CleanWebpackPlugin)
            }),
        ],
        optimization: {
            splitChunks: {
                cacheGroups: {
                    vendor: {
                        test: /[\\/]node_modules[\\/]/,
                        name: 'vendors',
                        chunks: 'all'
                    }
                }
            }
        }
    };
};