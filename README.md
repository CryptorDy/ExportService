# ExportService
Window service для регулярной выгрузки данных с БД в *.CSV файл.

Настройка экспорта данных осуществляется через конфигурационный файл. 
Доступна настройка экспорта данных либо один раз в стуки (один раз в несколько суток), либо один раз в час (один раз в несколько часов).
Количество одновременных выгрузок не ограничено

![image](https://user-images.githubusercontent.com/63358248/187203237-ae566b5f-5b13-4ec8-969c-b68c4fb7a92f.png)
![image](https://user-images.githubusercontent.com/63358248/187203291-4f129c83-fca9-4be0-9fde-6a44a39ccc92.png)


ВНИМАНИЕ!
Сервис имеет слабую защита от Sql инъекций!

Sql запросы строятся динамически и потенциально пользователь имеющий доступ к конфигурационному файлу может внедрить опасный запрос.

Защита: не допускаются определенные символы и ключевые слова и проходят проверку на допустимый формат 
