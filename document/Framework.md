# 腳本間關係說明

### DetectManager & PlayerManager & Player
* 由 DetectManager 主管偵測以及關節數據紀錄相關功能，搭配各個 Player 共同進行。
* 但 Player 不會直接放在 DetectManager ，因為並非只有 DetectManager 會使用到 Player 相關功能，且由動作偵測方面的腳本來管理也不合理。
* Player 會由 PlayerManager 統一管理，DetectManager 也是藉由 PlayerManager 來對 Player 進行操作。
* DetectManager 藉由 Player 來取得玩家 ID 和該動作的 Movement 來協助偵測。

### Player & Movement
* Player 透過 Pose 來存取 Movement，Movement 則是暫存著門檻值、正確率及是否通過，等其他協助偵測的功能(更新門檻值等)。

### DetectManager & MultiPosture
* DetectManager 透過 MultiPosture 根據 Pose 來載入比對標準。

### DetectManager & MovementDatas
* DetectManager 透過 MovementDatas 根據 Pose 來載入動作比對時的關節名稱。

### MovementDatas & MovementData
* MovementData 協助 MovementDatas 寫出、載入動作比對關節。
