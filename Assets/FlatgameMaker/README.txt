Flatgame Maker:
Made by Breogan Hackett for Dreamfeel.
Idea tool design and Sample scene music Llaura.
Sample scene art by Liadhg Young (additional art by breogan)
Inspiration and webcam import base code from Flatpack by Candle
Aditional code By Tim.


To see some examples you can navigate to Assets>FlatgameMaker>SampleScene and open FlatgameSampleScene
For an example of a very basic scene you can navigate to Assets>FlatgameMaker and open TemplateScene

There can be issues getting Textmesh Pro to play nice. You will probably be prompted to import the textmeshpro default resources on opening the sample scene. Once this is done you may need to reload the sample scene to get text to show up right (if it prompts you to save the scene when you are reloading do not save)

To use the Flatgame Maker window you need to go to Flatgame Maker>Flatgame Maker

To add a new area you can click the + button to the right of the "Current Area" UI in the Flatgame window, to add a new Flatgame Maker object you can click the + button to the right of the "Current Object" UI

To focus the scene view on objects or spaces when you navigate to them with the arrow buttons in the "Current Area" and "Current Object" UIs you can tick the checkboxes in the Center: []Areas []Objects When Moved To line

If you want to start in the area you are currently navigated to you can check the Start in current area checkbox. Note this will only work in the editor. To set the default starting area you will have to check the "Is Starting Area" checkbox on your desired area's inspector.

If you have questions or require support please make a post on the Flatgame Maker itch.io page https://dreamfeel.itch.io/flatgame

Flatgame Maker Feature List:
- Areas:
	These are sections of the unity scene difined by a a location and rectangular bounds use these for individual areas in your game.
	You can define different types of bounds collision for each area and these will interact with the bounmds and a second border n units in from the bounds where n is the "Bounds collision border" value
	You can add music to areas with the add music button.
- Player:
	Each area has a player and each player has indicvidual settings such as speed, input options or the option to flip or rotate the graphics based on where you're moving.
	By default the player has a rigidbody and collider so it can interact with triggers.
- Art Object:
	Art objects are images in the scene, these are rendered in front of objects they are above in the heirarchy and behind objects they are below
	By Default art objects will use our Flatgame Standard shader
	You can add triggers to these or make them into animations or add our juice script.
- Text:
	These have some of the same features as art objects but will use TextMeshPro text elements to display text in your game.
- Animations:
	These are frame by frame animations. Each frame is a child of the animation object.
	When playing the animation script will enable any renderers on the current frame or in its children and disable any renderers on the other frames.
	Selecting the animation object will let you preview the animation, change the framerate, navigate through frames and add and delete frames.
- Triggers:
	Triggers will trigger an action to happen whenever a payer enters their attatched collider2D
	Triggers can move between areas, move between unity scenes, Quit the game, Hide and or reveal objects, and also play audio clips.
- Juice:
	The Juice component lets you easily bring your art to life, allowing you to apply different movement, rotation and scaling effects.
	Spin lets you make an object spin at a constant speed
	Sine Transform lets you modify the opjects rotation, scale, or position based on sine waves.
	Shake lets you shake an object, you can tell it how fast and how much the objects should shake and you can determine how many time it will change its position per second.
	Move with camera lets you move objects relative to the movement of the camera you can use it to achieve effects like Parralax.
	Controlled spin lets you make something spin based on the players inputs.
- Graphic effects:
	You can change how your art looks based on a variety of features accessed in the "Edit Graphics" dropdown on Art Objects
	- Base Properties
		Here you can change basic things like how much the image tiles or if it pans and how fast it pans.
		You can also set it to use world space tiling. In this mode the scale of the main texture determines how big one tole of the image will be in units. This is usefuil for backgrounds and patterns.
	-Distortion
		Here you can enable distortion and change how it looks. I reccommend you use a Normal map for the Distrortion Texture.
		You can also setr how fast the distortion texture pans and if it uses World Spce tiling which works much like the World Space Tiling for the main texture.
	-Modify Colours
		Here you can change how the colours of your art look in various ways.
		You can modify the hue saturation and value of your image by enabling "Use HSV Colour Shift"
		You can remap dark areas of your image to one colour and light areas to anouther by enabling "Use Black-White Remapping"
		You can have your images transparency cut off suddenly instead of smoothly by enabling "Use Transparency Clipping"