## CreateBatchTask (產生 Job 入口)

- CreateBatchUploadNMQTask (墊一層)
    - CreateBatchUploadNMQTaskInner (又墊一層)
        - CreateBatchUploadEntity **(狀態 : WaitingToLoadData)**
        - 產一個 BatchuploadCode = **this._dbContext.Csp_GetSequencesCode("BatchUpload_Code")**
        - **塞 batchUpload Table**
        - **CreateBatchUploadNMQTask** : BatchUpload
