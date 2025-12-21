joBid = 97


## MY_QA

| Job_Id | Job_Name | Job_Description | Job_ClassName |
|--------|----------|-----------------|---------------|
| 97 | SendTemplateMail | 發送系統通知信 | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailProcess |
| 111 | SendTemplateMailPriorityHigh | 系統通知信(優先權高) | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailPriorityHighProcess |
| 112 | SendTemplateMailPriorityLow | 系統通知信(優先權低) | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailPriorityLowProcess |


## TWPROD


Job_Id	Job_Name	Job_Description
71	SendMail	寄送電子郵件
97	SendTemplateMail	發送系統通知信
112	SendTemplateMailPriorityLow	系統通知信(優先權低)


## Job 定義 (MYPROD)

| Job_Id | Job_Name | Job_Description | Job_ClassName |
|--------|----------|-----------------|---------------|
| 97 | SendTemplateMail | 發送系統通知信 | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailProcess |
| 111 | SendTemplateMailPriorityHigh | 系統通知信(優先權高) | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailPriorityHighProcess |
| 112 | SendTemplateMailPriorityLow | 系統通知信(優先權低) | NineYi.SCM.Frontend.NMQV2.Email.SendTemplateMailPriorityLowProcess |


## 其實 HIGH, LOW 就是 繼承

SendTemplateMailPriorityHighProcess : SendTemplateMailProcess