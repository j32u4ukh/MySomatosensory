# Player

玩家模型上會掛多個腳本，而外部腳本一律透過 Player 這一腳本來使用這些功能，包含其他腳本的功能但不限於此。

主要包含 AvatarController, PoseModelHelper, PlayerData 以及 Player 自身提供的函式。

## AvatarController

最為單純，取得模型的索引值，此數值為是先設定好(0, 1, ...)，當感測時，最先被感測到的玩家會操控編號 0 的模型，以此類推。

## PoseModelHelper

提供索引值、關節名稱和關節模型之間的轉換。

## PlayerData

紀錄偵測過程中會因人而異的部分，目前僅包含門檻值。

## RecordData

協助數據的紀錄與寫出，當中規定了每位玩家要記錄的數據，外部會透過 Player 來將數據存入。

檔案透過玩家 ID 、場景名稱和時間戳等來做區分。

## Player

設置玩家 ID ，數據紀錄以及讀取都是透過這個 ID 來區分不同玩家。

紀錄關節數據為 Player 和 DetectManager 共同作用的成果，而關節以外的數據(如正確率、開始時間、、、等)則是透過 Player 來寫入。

動作比對過程中會利用 Movement 協助暫時記住正確率等資訊，也是利用 Player 進行初始化。
