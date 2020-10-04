# Features: Meme

## Nudes

Sends a random image of grilled meat.

Command: `$nudes`

### Configuration

#### Model

```text
{
    "Path": string, // Path to directory with images.
    "AllowedImageTypes": string[] // List of extensions that are supported.
}
```

#### Example

```json
{
    "Path": "/images",
    "AllowedImageTypes": [ ".jpg", ".png", ".gif" ]
}
```

## Not nudes

Sends a random image of grilled vegetables.

Command: `$notnudes`

### Configuration

Configuration is same as `nudes` command.

## Peepolove

Generates peepo image with users avatar in his hands.

Command: `$peepolove`

### Configuration

Command peepolove needs configuration to generate images.

#### Model

```text
{
    "ProfilePicSize": ushort, // Discord avatar size
    "BasePath": string, // Path to image templates.
    "BodyFilename": string, // Filename of body part of image (ONLY FILENAME).
    "HandsFilename": string, // filename of hands part of image (ONLY FILENAME).
    "ProfilePicRect": Rectangle("X, Y, Width, Height"), // Size and position of user's profile image in peepo's hands.
    "Rotate": float, // Image rotation for better efect.
    "Screen": Rectangle("X, Y, Width, Height"), // Image original size
    "CropRect": Rectangle("X, Y, Width, Height") // Image size after crop useless parts.
}
```

##### Config example

```json
{
    "ProfilePicSize": 256,
    "BasePath": "wwwroot/Img/peepolove",
    "BodyFilename": "peepoBody.png",
    "HandsFilename": "peepoHands.png",
    "ProfilePicRect": "5, 312, 180, 180",
    "Rotate": 0.4,
    "Screen": "0, 0, 512, 512",
    "CropRect": "0, 115, 512, 397"
}
```

## Greet

Sends a greet with caller mention.

Command: `$grillhi`
Alias: `$hi`, `@GrillBot hi`

### Command: `$hi {mode}`

| Parameter | Type   | Description                                                      |
| --------- | ------ | ---------------------------------------------------------------- |
| mode      | string | Allowed modes are `text`, `bin`, `hex`, `2`, `8`, `10`, or `16`. |

## PeepoAngry

Generates PeepoAngry with profile picture of user (Caller or mentioned user).

### Configuration

#### Model

```text
{
    "ImagePath": string // Filename of peepoangry image.
}
```

##### Config example

```json
{
    "ImagePath": "wwwroot/Img/peepoangry.png"
}
```
