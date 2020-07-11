# Meme images

## Peepolove

Generates peepo image with users avatar in his hands.

### Command: `$peepolove`

### Configuration

Command peepolove needs configuration to generate images.

#### Model

```
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
    "BasePath": "wwwroot/img/peepolove",
    "BodyFilename": "peepoBody.png",
    "HandsFilename": "peepoHands.png",
    "ProfilePicRect": "5, 312, 180, 180",
    "Rotate": 0.4,
    "Screen": "0, 0, 512, 512",
    "CropRect": "0, 115, 512, 397"
}
```
