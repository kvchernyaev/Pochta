## Тестовое задание

Привет!
Нужно два приложения можно консольных, одно клиент – другое сервер.

Клиент:

Когда в «консоль» запущенного клиента вводят строку, она запоминается в локальную «базу» и отправляется на сервер. Если на сервер отправить не удалось, клиент периодически повторяет попытки отправки введенных в него строк. Даже после перезапуска программа, не теряя данных должна отправить данные на сервер.

Сервер:

Получает входящие строки сохраняет их в «базу», дополняя: ip адресом отправителя и временем получения. При вводе с клавиатуры в консоль “print”, красиво выводит в строки с ip-адресом и временем из «базы»


Ограничения:
```
1) Использовать C#
2) «База» удобный для тебя и надежный способ сохранения информации,
3) Можешь использовать любой надежный способ передачи данных по сети
4) Вместо консолей может быть Win Forms, Web, WPF
5) Код должен показывать тебя с лучшей стороны. Команда, делая ревью должна увидеть в тебе «своего» человека с которым хочется работать
```