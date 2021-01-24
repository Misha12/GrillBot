# Konfigurace

Bot obsahuje několik míst, kde se ukládají konfigurace.

- Globální konfigurace (v databázi)
- Statická konfigurace (pomocí appsettings.json, parametrů příkazové řádky, případně proměnné prostředí).
- Konfigurace metod volající z discord prostředí.
- Konfigurace oprávnění k metodám volajícím z discord prostředí
- Konfigurace modulů

## Globální konfigurace

Jedná se o konfiguraci, která ovlivňuje základní chod aplikace. Tyto konfigurace lze měnit za běhu pomocí příkazů z discord rozhraní. Položky uložené v globální konfiguraci jsou pevně dány výčtem [GlobalConfigItems](#globalconfigitems).

### Podpůrnné výčty

#### GlobalConfigItems

| Název                 | Hodnota | Popis                                                                                                                                          |
| --------------------- | :-----: | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| UnloadedModules       |    0    | Seznam modulů, které jsou v danou chvíli deaktivovány.                                                                                         |
| DisabledChannels      |    1    | Seznam kanálů (resp. identifikátorů), ve kterých se nemohou příkazy provádět.                                                                  |
| CommandPrefix         |    2    | Počáteční řetězec, kterým bot rozumí, že se jedná o příkaz. Pokud není zadán, tak se jako výchozí považuje znak `$`.                           |
| EmoteChain_CheckCount |    3    | Počet uživatelů, kteří musí napsat stejný emote, aby bot poslal také emote. Tzv. řetěz emotů. Pokud není zadán, tak bot takový řetěz ignoruje. |
| ActivityMessage       |    4    | Název zprávy, který se má zobrazovat jako aktivita bota.                                                                                       |
| ServerBoosterRoleId   |    5    | Identifikátor role, která označuje roli `Server Booster`.                                                                                      |
| AdminChannel          |    6    | Identifikátor kanálu, který označuje kanál pro administrátory.                                                                                 |
| LoggerChannel         |    7    | Identifikátor kanálu, kam logger zasílal zprávy. Již se nepoužívá.                                                                             |
| ErrorLogChannel       |    8    | Identifikátor kanálu, do kterého se budou zasílat chyby.                                                                                       |

### Příkazy pro práci s globální konfigurací.

*Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.*

#### $globalConfig keys

Získá seznam všech dostupných klíčů konfigurace (jedná se o položky výčtu [GlobalConfigItems](#globalconfigitems)).

##### Příklad volání

```sh
$globalConfig keys
```

#### $globalConfig get `{key}`

Získá hodnotu uloženou pod zadaným konfiguračním klíčem. 

##### Parametry

| Název |               Datový typ                | Popis                                                                   |
| ----- | :-------------------------------------: | ----------------------------------------------------------------------- |
| key   | [GlobalConfigItems](#globalconfigitems) | Klíč reprezentující výčtovou položku příslušné konfigurační vlastnosti. |

##### Příklad volání

```sh
$globalConfig get CommandPrefix
```

#### $globalConfig set `{key}` `{value}`

Nastaví hodnotu pod zadaným konfiguračním klíčem.

##### Parametry

| Název |               Datový typ                | Popis                                                                   |
| ----- | :-------------------------------------: | ----------------------------------------------------------------------- |
| key   | [GlobalConfigItems](#globalconfigitems) | Klíč reprezentující výčtovou položku příslušné konfigurační vlastnosti. |
| value |                 string                  | Hodnota, která bude uložená pod danou konfigurační vlastností.          |

##### Příklad volání

```sh
$globalConfig set CommandPrefix !
```

### Schéma databázových tabulek.

#### Tabulka `GlobalConfig`

- Sloupec primárního klíče: `Key`

| Název sloupce | Datový typ (.NET) | Datový typ (SQL) | Nullable | Popis                                                                                                        |
| ------------- | :---------------: | :--------------: | :------: | ------------------------------------------------------------------------------------------------------------ |
| Key           |      string       |  nvarchar(450)   |    Ne    | Unikátní klíč konfigurační položky. Klíč je odvozen z položek výčtu [GlobalConfigItems](#globalconfigitems). |
| Value         |      string       |   VARCHAR(MAX)   |    Ne    | Konfigurační hodnota.                                                                                        |

## Statická konfigurace

Statická konfigurace obsahuje citlivé konfigurační hodnoty, které se nesmí měnit za běhu a nemohou být sdíleny mezi ostatními. Tato položky lze načítat pomocí proměnného prostředí, souborů appsettings.{environment}.json, nebo třeba také pomocí parametrů příkazové řádky.

### Položky statické konfigurace

| Název                  | Popis                                                        |
| ---------------------- | ------------------------------------------------------------ |
| APP_TOKEN              | Přístupový token reprezentující uživatele bota.              |
| DB_CONN                | Připojovací řetězec k databázi.                              |
| ASPNETCORE_ENVIRONMENT | Typ proměnného prostředí (Development, Staging, Production). |
| ASPNETCORE_URLS        | Adresa, na které má naslouchat webový server Kestrel.        |

## Konfigurace příkazů a oprávnění

Konfigurace příkazů je uložena v databázi. Konkrétně ve speciální tabulce `MethodConfig`. Pokud nějaká metoda (příkaz) vyžaduje konkrétní nastavení, lze jej zde uložit v podobě JSON dat.
K této tabulce je vázaná také tabulka `MethodPerms`, která obsahuje konfiguraci oprávnění jednotlivých rolí a uživatelů.

### Příkazy pro práci s konfigurací příkazů a oprávnění.

*Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.*
Pokud nebude řečeno jinak, tak se všechny příkazy a data vztahují k serveru, ve kterém byl zavolán příkaz.

#### $config addMethod `{command}` `{onlyAdmins}` `configJson`

Vytvoření konfigurace k metodě.

*Vytvoření konfigurace a oprávnění metody `$config addMethod` musí provést administrátor bota. Jiný uživatel nemá přístup.*

##### Parametry

| Název      | Datový typ | Popis                                                                                                  |
| ---------- | :--------: | ------------------------------------------------------------------------------------------------------ |
| command    |   string   | Identifikace metody, pod kterou se volá. Zadává se ve formátu `{group}/{command}`. Lomítko je povinné. |
| onlyAdmins |    bool    | Příznak, že tato metoda je dostupná pouze administrátorům bota.                                        |
| configJson |  JObject   | Konfigurační data pro danou metodu ve formátu JSON.                                                    |

##### Příklad volání

```sh
$config addMethod config/addMethod false {}
```

#### $config list

Vypíše stránkovaný seznam všech konfigurací metod.

##### Příklad volání

```sh
$config list
```

#### $config switchOnlyAdmins `{method}` `{onlyAdmins}`

Přepnutí metody, aby byla dostupná pouze pro administrátory bota.

##### Parametry

| Název      |               Datový typ                | Popis                                                                                                     |
| ---------- | :-------------------------------------: | --------------------------------------------------------------------------------------------------------- |
| method     | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`. |
| onlyAdmins |                  bool                   | Příznak, že tato metoda je dostupná pouze administrátorům bota.                                           |

##### Příklad volání

```sh
$config switchOnlyAdmins config/addMethod true
```

#### $config updateJson `{method}` `{json}`

Úprava JSON konfigurace metody.

##### Parametry

| Název  |               Datový typ                | Popis                                                                                                     |
| ------ | :-------------------------------------: | --------------------------------------------------------------------------------------------------------- |
| method | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`. |
| json   |                 JObject                 | Konfigurační data pro danou metodu ve formátu JSON.                                                       |

##### Příklad volání

```sh
$config updateJson config/addMethod {"someData": true}
```

#### $config addPermission `{method}` `{targetID}` `{permType}` `{allowType}`

Vytvoření oprávnění k dané metodě.

##### Parametry

| Název     |               Datový typ                | Popis                                                                                                                                              |
| --------- | :-------------------------------------: | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| method    | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`.                                          |
| targetID  |                 string                  | Discord identifikátor příslušné role, nebo uživatele. Případně je možné zadat řetězec `everyone`, pokud je požadováno zadat oprávnění pro všechny. |
| permType  |                   int                   | Druh cílové skupiny, pro kterou se vztahuje toto oprávnění. Řídí se výčtem [PermType](#permtype).                                                  |
| allowType |                   int                   | Označení, zda je přístup povolen, či zakázán. Řídí se výčtem [AllowType](#allowtype)                                                               |

##### Příklad volání

```sh
$config addPermission config/addMethod 370506820197810176 0 0
```

#### $config getMethod `{method}`

Získání konfigurace hledané metody. Vrací také konfiguraci oprávění pro danou metodu.

##### Parametry

| Název  |               Datový typ                | Popis                                                                                                     |
| ------ | :-------------------------------------: | --------------------------------------------------------------------------------------------------------- |
| method | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`. |

##### Příklad volání

```sh
$config getMethod config/addMethod
```

#### $config removePermission `{method}` `{permID}`

Odebrání oprávnění k metodě.

##### Parametry

| Název  |               Datový typ                | Popis                                                                                                                  |
| ------ | :-------------------------------------: | ---------------------------------------------------------------------------------------------------------------------- |
| method | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`.              |
| permID |                   int                   | Unikátní identifikátor oprávnění. Lze zjistit pomocí příkazu [$config getMethod `{method}`](#config-getmethod-method). |

##### Příklad volání

```sh
$config removePermission config/addMethod 42
```

#### $config getJson `{method}`

Získá aktuální JSON konfiguraci k metodě.

##### Parametry

| Název  |               Datový typ                | Popis                                                                                                     |
| ------ | :-------------------------------------: | --------------------------------------------------------------------------------------------------------- |
| method | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`. |

##### Příklad volání

```sh
$config getJson config/addMethod
```

#### $config removeMethod `{method}`

Smazání konfigurace metody (vč. oprávnění).

##### Parametry

| Název  |               Datový typ                | Popis                                                                                                     |
| ------ | :-------------------------------------: | --------------------------------------------------------------------------------------------------------- |
| method | [GroupCommandMatch](#groupcommandmatch) | Definice metody. Je možné zadat identifikátor metody, případně textovou reprezentaci `{group}/{command}`. |

##### Příklad volání

```sh
$config removeMethod config/addMethod
```

#### $config removeGuild `{guildID}`

Smazání všech konfigurací (vč. oprávnění) k serveru, jehož identifikátor je uveden v parametru.

##### Parametry

| Název   | Datový typ | Popis                                                                   |
| ------- | :--------: | ----------------------------------------------------------------------- |
| guildID |   ulong    | Identifikátor serveru, ze kterého se mají veškeré konfigurace vyčistit. |

##### Příklad volání

```sh
$config removeGuild 461541385204400138
```

#### $config export

Export všech konfigurací (vč. oprávnění) uložených v databázi. Výsledná data jsou uloženy do JSON souboru a odesláno do soukromých zpráv.

##### Příklad volání

```sh
$config export
```

#### $config import

Import konfigurace z JSON souboru. Metoda neočekává běžný vstup, ale přílohu ve zprávě se zavolaným příkazem.

##### Příklad volání

```sh
$config import
```
a příloha

#### $config rename `{id}` `{group}` `{command}`

Přejmenování metody pro správné propárování s existjícím příkazem.

##### Parametry

| Název   | Datový typ | Popis                                                                |
| ------- | :--------: | -------------------------------------------------------------------- |
| id      |    int     | Unikátní identifikátor záznamu, ke kterému se přejmenování vztahuje. |
| group   |   string   | Nový název skupiny příkazů.                                          |
| command |   string   | Nový název příkazu.                                                  |

##### Příklad volání

```sh
$config rename 42 config addMethod
```

### Podpůrné třídy

#### GroupCommandMatch

Pomocná třída sloužící jako vstupní parametr pro metodu.

##### Vlastnosti

| Název    |     Datový typ      | Popis                                                                                                                                     |
| -------- | :-----------------: | ----------------------------------------------------------------------------------------------------------------------------------------- |
| MethodID | Nullable&lt;int&gt; | Unikátní identifikátor nakonfigurované metody pro daný server. Obsahuje NULL, pokud metoda ještě nebyla na daném serveru nakonfigurována. |
| Group    |       string        | Skupina příkazů, ve kterém se příkaz nachází.                                                                                             |
| Command  |       string        | Konkrétní identifikace metody v rámci skupiny příkazů.                                                                                    |

### Schéma databázových tabulek

#### Tabulka `MethodsConfig`

- Sloupec primárního klíče: `ID`

| Název sloupce | Datový typ (.NET) | Datový typ (SQL) | Nullable | Popis                                                                  |
| ------------- | :---------------: | :--------------: | :------: | ---------------------------------------------------------------------- |
| ID            |        int        |       INT        |    Ne    | Unikátní identifikátor metody.                                         |
| GuildID       |      string       |   VARCHAR(30)    |    Ne    | Identifikátor serveru, na kterém je metoda nakonfigurována.            |
| Group         |      string       |   VARCHAR(100)   |    Ne    | Skupina metod, ve které se konkrétní metoda nachází.                   |
| Command       |      string       |   VARCHAR(100)   |    Ne    | Název konkrétní metody v rámci skupiny příkazů.                        |
| ConfigData    |      string       |   VARCHAR(MAX)   |    Ne    | Konfigurační data pro danou metodu. Data jsou ve formátu JSON.         |
| OnlyAdmins    |       bool        |       BIT        |    Ne    | Konfigurační příznak, že tato metoda je pouze pro administrátory bota. |
| UsedCount     |       long        |      BIGINT      |    Ne    | Počet použití dané metody.                                             |

#### Tabulka `MethodPerms`

- Sloupec primárního klíče: `PermID`

| Název sloupce |    Datový typ (.NET)    | Datový typ (SQL) | Nullable | Popis                                                                            |
| ------------- | :---------------------: | :--------------: | :------: | -------------------------------------------------------------------------------- |
| PermID        |           int           |       INT        |    Ne    | Unikátní identifikátor oprávnění.                                                |
| MethodID      |           int           |       INT        |    Ne    | ID metody, na kterou se váže toto oprávnění. Cizí klíč do tabulky `MethodConfig` |
| DiscordID     |         string          |   VARCHAR(30)    |    Ne    | Discord identifikátor příslušné role, nebo uživatele.                            |
| PermType      |  [PermType](#permtype)  |     TINYINT      |    Ne    | Druh cílové skupiny, pro kterou se vztahuje toto oprávnění.                      |
| AllowType     | [AllowType](#allowtype) |     TINYINT      |    Ne    | Označení, zda je přístup povolen, či zakázán.                                    |

### Podpůrné výčty

#### PermType

Určení cílové skupiny pro kterou se vztahuje toto oprávnění.

| Název    | Hodnota | Popis                                |
| -------- | :-----: | ------------------------------------ |
| Role     |    0    | Oprávnění náleží k roli.             |
| User     |    1    | Oprávnění náleží k uživateli.        |
| Everyone |    2    | Oprávnění náleží ke všem uživatelům. |

#### AllowType

Označení, zda je přístup povolen, či zakázán.

| Název | Hodnota | Popis            |
| ----- | :-----: | ---------------- |
| Allow |    0    | Přístup povolen. |
| Deny  |    1    | Přístup zakázán. |

## Konfigurace modulů

Bot se skládá s řady modulů, ve kterých se nacházejí metody, které jsou volány příkazy ve zprávách. Tyto module je možné různě vypínat a zapínat podle potřeby.

Všechny moduly jsou ve výchozím stavu aktivované. Deaktivované moduly se ukládají do tabulky `GlobalConfig` pod klíčovou hodnotou `UnloadedModules`.

### Příkazy

*Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.*

#### $modules list

Získání stránkovaného seznamu všech modulů a jejich stavu.

#### $modules add `{name}`

Aktivace deaktivovaného modulu.

##### Parametry

| Název | Datový typ | Popis         |
| ----- | :--------: | ------------- |
| name  |   string   | Název modulu. |

##### Příklad volání

```sh
$modules add ConfigModule
```

#### $modules remove `{name}`

Dekativace aktivovaného modulu.

##### Parametry

| Název | Datový typ | Popis         |
| ----- | :--------: | ------------- |
| name  |   string   | Název modulu. |

##### Příklad volání

```sh
$modules remove ConfigModule
```
