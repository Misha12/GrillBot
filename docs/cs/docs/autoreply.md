# Automatické odpovědi

Automatické odpovědi umožňují na základě obsahu zprávy, nebo celé zprávy odpovědět určitým přednastaveným textem.

Data jsou databázi uloženy v tabulce `AutoReply`. Tato tabulka slouží jako perzistentní úložiště a data z této tabulky jsou načítány pouze při startu aplikace.
Poté jsou uloženy v paměti a čtení probíhá čistě z paměti.

Při ukládání dat (přidání/editace/...) se nejprve data ukládají v databázi a poté až v paměti. To z toho důvodu, aby nedošlo k nekonzistencím, pokud by došlo k případné chybě.

## Příkazy

*Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.*

### $autoreply list

Vypíše stránkovaný seznam všech definovaných automatických odpovědí. V seznamu je kromě dat uložených v databázi navíc ještě počítadlo použití dané automatické odpovědi. Tzn. kolikrát se povedla shoda se zprávou.

### $autoreply disable `{id}`

Provede se deaktivace automatické odpovědi. Pokud je již automatická odpověď deaktivována, pak se o tom vrátí příslušné hlášení a celá akce je zrušena.

#### Parametry

| Název | Datový typ | Popis                                                                                                                           |
| :---: | :--------: | ------------------------------------------------------------------------------------------------------------------------------- |
|  id   |    int     | ID automatické odpověďi. Správné ID automatické odpovědi je možné získat z výsledku příkazu [$autoreply list](#autoreply-list). |

#### Příklad volání

```sh
$autoreply disable 42
```

### $autoreply enable `{id}`

Opak příkazu [$autoreply disable `{id}`](#autoreply-disable-id). Provede aktivaci automatické odpovědi, pokud není aktivní.

#### Parametry

| Název | Datový typ | Popis                                                                                                                           |
| :---: | :--------: | ------------------------------------------------------------------------------------------------------------------------------- |
|  id   |    int     | ID automatické odpověďi. Správné ID automatické odpovědi je možné získat z výsledku příkazu [$autoreply list](#autoreply-list). |

#### Příklad volání

```sh
$autoreply enable 42
```

### $autoreply add `{data}`

Přidání nové automatické odpovědi. Tato metoda vyžaduje specificky formátovaný řetězec obsahující data automatické odpovědi. Ukázku těchto dat lze získat z příkazu [$autoreply example](#autoreply-example).

#### Parametry

| Název | Datový typ | Popis                                                                  |
| :---: | :--------: | ---------------------------------------------------------------------- |
| data  |   string   | Specificky naformátovaný řetězec obsahující data automatické odpovědi. |

#### Příklad volání

````sh
$autoreply add ```
MustContains
```
```
ReplyMessage
```
==
0
*
````

### $autoreply edit `{id}` `{data}`

Provede přepis již existující automatické odpovědi.

#### Parametry

| Název | Datový typ | Popis                                                                                                                                            |
| :---: | :--------: | ------------------------------------------------------------------------------------------------------------------------------------------------ |
|  id   |    int     | ID automatické odpověďi. Správné ID automatické odpovědi je možné získat z výsledku příkazu [$autoreply list](#autoreply-list).                  |
| data  |   string   | Specificky naformátovaný řetězec obsahující data automatické odpovědi. Je totožný jako řetězec v [$autoreply add `{data}`](#autoreply-add-data). |

#### Příklad volání

````sh
$autoreply edit 42 ```
MustContains
```
```
ReplyMessage
```
==
0
*
````

### $autoreply remove `{id}`

Odebere existující automatickou odpověď.

#### Parametry

| Název | Datový typ | Popis                                                                                                                           |
| :---: | :--------: | ------------------------------------------------------------------------------------------------------------------------------- |
|  id   |    int     | ID automatické odpověďi. Správné ID automatické odpovědi je možné získat z výsledku příkazu [$autoreply list](#autoreply-list). |

#### Příklad volání

```sh
$autoreply remove 42
```

### $autoreply example

Vrátí šablonová data pro metody [$autoreply add `{data}`](#autoreply-add-data) a [$autoreply edit `{id}` `{data}`](#autoreply-edit-id-data).

#### Příklad volání

```sh
$autoreply example
```

## Ukázka

![Example](img/autoreply_example.png)

## Schéma databázových tabulek

### Tabulka `AutoReply`

- Sloupec primárního klíče: `ID`

| Název sloupce |                Datový typ (.NET)                | Datový typ (SQL) | Nullable | Popis                                                                                                                                                                            |
| :-----------: | :---------------------------------------------: | :--------------: | :------: | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|      ID       |                       int                       |       INT        |    Ne    | Unikátní identifikátor automatické odpovědi. Přiřazován automaticky SQL serverem.                                                                                                |
| MustContains  |                     string                      |   varchar(max)   |    Ne    | Text zprávy, který musí zpráva obsahovat.                                                                                                                                        |
| ReplyMessage  |                     string                      |   varchar(max)   |    Ne    | Text zprávy, který bot při shodě odešle.                                                                                                                                         |
|  CompareType  | [AutoReplyCompareTypes](#autoreplycomparetypes) |       int        |    Ne    | Určení metody, jakou se má provést porovnání zprávy s požadovaným textem.                                                                                                        |
|    GuildID    |                     string                      |   varchar(30)    |    Ne    | (Snowflake) Identifikátor serveru, ve kterém se porovnávání a odpovídání má provádět.                                                                                            |
|   ChannelID   |                     string                      |     varchar      |   Ano    | (Snowflake) Identifikátor kanálu, ve kterém se porovnání a odpovídání má provádět. Pokud má bot tuto zprávu tímto textem kontrolovat všude, pak je v tomto sloupci uloženo NULL. |
|     Flags     |                       int                       |       int        |    Ne    | Konfigurační příznaky automatické odpovědi. Příznaky se řídí výčtem [AutoReplyParams](#autoreplyparams).                                                                         |

### Pomocné výčty

#### AutoReplyCompareTypes

|  Název   | Hodnota | Popis                                                                                          |
| :------: | :-----: | ---------------------------------------------------------------------------------------------- |
| Contains |    0    | Prověřovaný text (neboli text nacházející se v `MustContains`) se musí objevit v textu zprávy. |
| Absolute |    1    | Prověřovaný text musí být shodný s textem zprávy.                                              |

#### AutoReplyParams

|     Název     | Hodnota | Popis                                                                                                                                                                               |
| :-----------: | :-----: | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|     None      |    0    | Pomocná výčtová položka příznaků. Nepoužívá se.                                                                                                                                     |
| CaseSensitive |    1    | Při nastavení tohoto bitu se při porovnávání se zohledňují velké a malé znaky.                                                                                                      |
|   Disabled    |    2    | Při nastavení tohoto bitu se tato automatická odpověď ignoruje. Neprobíhá porovnávání a odpovídání.                                                                                 |
|  AsCodeBlock  |    4    | Při nastavení tohoto bitu se odpověď posílá v bloku kódu. Více viz [Markdown Code Block](https://docs.github.com/en/github/writing-on-github/creating-and-highlighting-code-blocks) |
