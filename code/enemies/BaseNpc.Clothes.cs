using System;
using System.Linq;
using Sandbox;

namespace FearfulCry.Enemies;

public partial class BaseNpc
{
    public ClothingContainer Clothing { get; protected set; }
	private static Color DefaultRenderColor => Color.White;

    /// <summary>
	/// Dress dresses the entity with what's inside the clothing container.
	/// It's recommended to call UpdateClothes() before this.
	/// 
	/// Params are optional and support null conditionals.
	/// </summary>
	/// <param name="renderingColor">Rendering color of the skin</param>
	/// <param name="clothesColor">Clothes color</param>
	/// <example>
	/// Dress(Color.Blue, Color.Parse("#4dc223"))
	/// </example>
	public async void Dress(Color? renderingColor = default, Color? clothesColor = default)
    {   
        // If either color params are default or invalid then override with default white.
        if (renderingColor.GetValueOrDefault().Equals(default)) {
			renderingColor = DefaultRenderColor;
		}
        if (clothesColor.GetValueOrDefault().Equals(default)) {
			clothesColor = DefaultRenderColor;
		}

        // Reduce the chance of skins not working.
		ClearMaterialOverride();

        // Return is invalid.
        if (!this.IsValid())
			return;

        // Set rendering color to param.
		RenderColor = renderingColor.Value;
        // Dress the citizen model.
		Clothing.DressEntity( this );

        // Loop through the clothing items.
        foreach(var clothing in Children.OfType<ModelEntity>()) {
            if (clothing.Tags.Has("clothes")) {
				// Set the color of the clothing items.
				clothing.RenderColor = clothesColor.Value;
			}
        }

        //
        // Hacky shit below...
        //

		await Task.Delay( 1000 );
        if (!this.IsValid())
			return;

		await Task.Delay( 200 );
		ClearMaterialOverride();
        if (!this.IsValid())
			return;


		var SkinMaterial = Clothing.Clothing.Select( x => x.SkinMaterial ).Select( x => Material.Load( x ) ).FirstOrDefault();
		// var EyesMaterial = Clothing.Clothing.Select( x => x.EyesMaterial ).Select( x => Material.Load( x ) ).FirstOrDefault();

		SetMaterialOverride( SkinMaterial, "skin" );

        //? This line causes some odd wireframing on remote clients.
		// SetMaterialOverride( EyesMaterial, "eyes" );
	}

    /// <summary>
	/// Randomize the clothing wardrobe of this citizen.
	/// </summary>
    public virtual void UpdateClothes()
    {
        if (Clothing == null) {
			Clothing = new();
		}

		Clothing item;
		String model;

        //
        // skin
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/skin01.clothing",
                "models/citizen_clothes/skin02.clothing",
                "models/citizen_clothes/skin03.clothing",
                "models/citizen_clothes/skin04.clothing",
                "models/citizen_clothes/skin05.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}

        //
        // hair
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/hair/hair_balding/hair_baldingbrown.clothing",
                "models/citizen_clothes/hair/hair_balding/hair_baldinggrey.clothing",
                "models/citizen_clothes/hair/hair_bobcut/hair_bobcut.clothing",
                "models/citizen_clothes/hair/hair_bun/hair_bun.clothing",
                "models/citizen_clothes/hair/hair_fade/hair_fade.clothing",
                "models/citizen_clothes/hair/hair_longbrown/Models/hair_longblack.clothing",
                "models/citizen_clothes/hair/hair_longbrown/Models/hair_longbrown.clothing",
                "models/citizen_clothes/hair/hair_longbrown/Models/hair_longgrey.clothing",
                "models/citizen_clothes/hair/hair_longcurly/hair_longcurly.clothing",
                "models/citizen_clothes/hair/hair_looseblonde/hair.loose.blonde.clothing",
                "models/citizen_clothes/hair/hair_looseblonde/hair.loose.brown.clothing",
                "models/citizen_clothes/hair/hair_looseblonde/hair.loose.grey.clothing",
                "models/citizen_clothes/hair/hair_mullet/hair_mullet.clothing",
                "models/citizen_clothes/hair/hair_ponytail/ponytail.clothing",
                "models/citizen_clothes/hair/hair_wavyblack/hair_wavyblack.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}

        //
        // eyebrow
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/hair/eyebrows/eyebrows.clothing",
                "models/citizen_clothes/hair/eyebrows_bushy/eyebrows_bushy.clothing",
                "models/citizen_clothes/hair/eyebrows_drawn/eyebrows_drawn.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}

        //
        // facial
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/hair/moustache/moustache_brown.clothing",
                "models/citizen_clothes/hair/moustache/moustache_grey.clothing",
                "models/citizen_clothes/hair/scruffy_beard/scruffy_beard_black.clothing",
                "models/citizen_clothes/hair/scruffy_beard/scruffy_beard_brown.clothing",
                "models/citizen_clothes/hair/scruffy_beard/scruffy_beard_grey.clothing",
                "models/citizen_clothes/hair/stubble/stubble.clothing",
                "models/citizen_clothes/hair/eyelashes/eyelashes.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}

        //
        // chest
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/shirt/Army_Shirt/army_shirt.clothing",
                "models/citizen_clothes/shirt/Chainmail/chainmail.clothing",
                "models/citizen_clothes/shirt/Flannel_Shirt/flannel_shirt.clothing",
                "models/citizen_clothes/shirt/Hawaiian_Shirt/Hawaiian Shirt.clothing",
                "models/citizen_clothes/shirt/Jumpsuit/blue_jumpsuit.clothing",
                "models/citizen_clothes/shirt/Jumpsuit/prison_jumpsuit.clothing",
                "models/citizen_clothes/shirt/Longsleeve_Shirt/longsleeve_shirt.clothing",
                "models/citizen_clothes/shirt/Priest_Shirt/priest_shirt.clothing",
                "models/citizen_clothes/shirt/Tanktop/tanktop.clothing",
                "models/citizen_clothes/shirt/V_Neck_Tshirt/v_neck_tshirt.clothing",

                "models/citizen_clothes/vest/Cardboard_Chest/cardboard_chest.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}

        //
        // trousers
        //
		model = Rand.FromArray( new[]
			{
                "models/citizen_clothes/trousers/CargoPants/cargo_pants_army.clothing",
                "models/citizen_clothes/trousers/Jeans/jeans.clothing",
                "models/citizen_clothes/trousers/SmartTrousers/trousers.smart.clothing",
			} );
        if (ResourceLibrary.TryGet<Clothing>(model, out item)) {
			Clothing.Clothing.Add( item );
		}
	}

}