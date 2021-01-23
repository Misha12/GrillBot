# Narozeniny

Narozeniny umožňují uchovávat datum narození uživatelů a poté pomocí příkazu získávat informaci, zda daný uživatel má narozeniny.

Uložení data narození je čistě dobrovolné. Bot neumožňuje vrátit seznam všech dat narození vzhledem k uživateli.

Data narození jsou uložena v tabulce `DiscordUsers`, která je popsána v sekci [Správa uživatelů](../users).

## Příkazy

Všechny níže uvedené příkazy počítají s výchozím prefixem `$`. Prefix se může lišit v závislosti na vaší konfiguraci.

### $birthday

Získá stránkovaný seznam všech uživatelý, kteří k aktuálnímu dni mají narozeniny. Pokud při přidání data narození zadali rok narození, tak je zde zobrazen i jejich věk.

### $birthday add `{date}`

Provede uložení data narození. Uložení se vždy váže k uživateli, který příkaz zavolal. Není možné přidat datum narození za jiného uživatele.

#### Parametry

| Název | Datový typ | Popis                                                                                                                                                                           |
| :---: | :--------: | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| date  |   string   | Datum narození uživatele, který zavolal příkaz pro přidání. Lze zadat ve formátu `dd/MM/yyyy`, nebo `dd/MM`. Pokud bude zadán i rok, tak se v daný den narozenin zobrazí i věk. |

#### Příklad volání

```sh
$birthday add 1/10/2020
```

nebo

```sh
$birthday add 1/10
```

### $birthday remove

Pokud si již uživatel nepřeje mít uložený datum narození (a případně mít zobrazované narozeniny), tak může tímto příkazem provést jeho odebrání. Odebrání se vždy váže k uživateli, který příkaz zavolal. Není možné odebrat datum narození za jiného uživatele.

#### Příklad volání

```sh
$birthday remove
```

### $birthday have?

Zjistí, zda má uživatel uložený v databázi datum narození. Například pokud si není jistý a nechce provádět přidání, nebo smazání data narození.

#### Príklad volání

```sh
$birthday have?
```
