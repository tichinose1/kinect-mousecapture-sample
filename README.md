# 内容
- Kinectを使って手でマウスカーソルを動かす
- Kinectで取得できる骨格データの座標を画面座標に変換
- .NETのプラットフォーム呼び出しでカーソル移動、クリック
- ディスプレイに触れた手の位置とカーソルの位置が一致するように特定PCに最適化（一致してない）
- 他PCで試す場合はSettingsでパラメータ要調整

## KinectMouseCursorConsole
- コンソールアプリケーション
- バックグラウンドでKinectサービスに問い合わせ

# 環境
- Visual Studio 2015
- .NET Framework 4.6.1
- C# 6.0
- Kinect SDK 2.0
- Ractive Extensions 3.0

# TODO
- [ ] 精度向上（悪過ぎ）
- [ ] 音声の投入
- [ ] Rx無し版の実装