# 我らサンタクロース一等兵
使用技術
----
<img src="https://img.shields.io/badge/-Unity-000000.svg?logo=unity&style=plastic">
<img src="https://img.shields.io/badge/-C%20sharp-11B48A.svg?logo=vine&style=plastic">
<img src="https://img.shields.io/badge/-HLSL-25D366.svg?logo=&style=plastic">
<img src="https://img.shields.io/badge/-Visual%20Studio-007ACC.svg?logo=visualstudiocode&style=plastic">

概要
----
ステルスゲームかつ3DでTPSのパズルゲームを組み合わせた形のゲーム   
参考にするゲームはPortalとMetalGearSolit
開発環境はUnity   
攻撃を行わない誘導だけでステルスをしつつゴールに辿り着くという形   
発見されたら即ゲームオーバー   
ステージは選択型でLevelの低いステージをクリアすると次が開放される   
SceneはMainSceneのみでここにPrefabの形でステージを用意する   
Scene遷移は重くなりがちで、Prefabを読み込む型ならaddressableで非同期読み込みによりスタート地点から優先的に読み込むなどの工夫が可能   
外観としてはローポリでライティングはリッチに行う形   
LightingはBakerlyを使用   
とりあえずサンタクロースモチーフだが時期がすぎるため、エイリアンなどの季節に関係ないものに置き換え予定   
レンダーパイプラインは使い慣れているURPで   
Animationを丁寧に作る事でリッチ感が出る   

テストプレイ
---
※ v1.0.3-alpha   
<img src="Document/TestPlay_00.png" width = 50%>

ゲーム全体のフロー
----
※GameManagerで全体のフローを管理する古いバージョンのもの   
**ゲーム全体の流れ**   
<img src="Document/GameStream.png" width = 70%>
   
**敵AIルーチン**   
<img src="Document/AIFlow.png" width = 70%>

TODO   
---
- 当初開発速度を要求されたためゲームにおける各Controller間の実行をシングルトンなGameManager上に実装   
下記で実装するKeyboardによるUI操作等もGameManagerに一任するとGameManagerが肥大化するため、GameStream classを実装する
- ステージセレクトに対応できていない(StartPanelにOSAを利用したListを作成)
- ゲーム中のescによる一時停止及びrestart, return to start panelの実装   
内部ではdebug用にPlayer及びEnemyの一時停止は実装済み   
またpostprocessingのGaussianBlurも実装済み   
残りはPausePanelのみとなる   
- ゲームの設定UIの実装   
現在はベタでコードに書いている内容をUIで変更可能なUIの実装   
- 敵に後ろから接触した際の強制発見   
Unit同士のlayerはぶつからない仕様にして、ぶつかった際にアニメーション再生と後ろへの回転を行う
- 壁に張り付くモーションと移動   
- Messageなどの条件に応じて表示するものを今は表示数も少なく開発速度を上げるため、各コンポーネントのインスペクタに書いているが   
これを以前作ったNode形式のイベント管理システム EventNodesに置き換える   


----

©2024 Daiki Ito