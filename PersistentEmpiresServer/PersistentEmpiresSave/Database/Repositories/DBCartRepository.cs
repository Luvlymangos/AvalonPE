using Dapper;
using Database.DBEntities;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBCartRepository
    {
        //public static void Initialize()
        //{
        //    SaveSystemBehavior.OnGetAllCarts += GetAllCarts;
        //    SaveSystemBehavior.OnCreateOrSaveCart += CreateOrSaveCart;
        //}

        //private static DBCart CreateDBCart(PE_AttachToAgent Cart)
        //{
        //    Debug.Print("[Save Module] CREATE DB Cart (" + Cart != null ? " " + Cart.GetHashCode() : "Cart IS NULL !)");
        //    return new DBCart
        //    {
        //        Id = Cart.ID,
        //        PosX = (int)Cart.GameEntity.GlobalPosition.x,
        //        PosY = (int)Cart.GameEntity.GlobalPosition.y,
        //        PosZ = (int)Cart.GameEntity.GlobalPosition.z,
        //        Prefab = Cart.GameEntity.GetPrefabName(),
        //        InventoryID = Cart.AttachedInventoryID
        //    };
        //}
        //public static IEnumerable<DBCart> GetAllCarts()
        //{
        //    Debug.Print("[Save Module] LOADING ALL Carts FROM DB");
        //    return DBConnection.Connection.Query<DBCart>("SELECT * FROM carts");
        //}

        //public static DBCart GetCart(PE_AttachToAgent Cart)
        //{
        //    Debug.Print("[Save Module] LOAD Cart FROM DB (" + Cart.ID + ")");
        //    IEnumerable<DBCart> result = DBConnection.Connection.Query<DBCart>("SELECT * FROM carts WHERE ID = @ID", new { ID = Cart.ID });
        //    Debug.Print("[Save Module] LOAD Cart FROM DB (" + Cart.ID + ") RESULT COUNT " + result.Count());
        //    if (result.Count() == 0) return null;
        //    return result.First();
        //}

        //public static DBCart CreateOrSaveCart(PE_AttachToAgent Cart)
        //{

        //    if (GetCart(Cart) == null)
        //    {
        //        return CreateCart(Cart);
        //    }
        //    return SaveCart(Cart);
        //}
        //public static DBCart CreateCart(PE_AttachToAgent Cart)
        //{
        //    Debug.Print("[Save Module] CREATE Cart TO DB (" + Cart != null ? " " + Cart.ID : "Cart IS NULL !)");
        //    DBCart dbcart = CreateDBCart(Cart);
        //    string insertQuery = "INSERT INTO carts (Id, PosX, PosY, PosZ, Prefab, InventoryId) VALUES (@ID @PosX, @PosY, @PosZ, @Prefab, @InventoryID)";
        //    DBConnection.Connection.Execute(insertQuery, dbcart);
        //    Debug.Print("[Save Module] CREATED Cart TO DB (" + Cart != null ? " " + Cart.ID : "Cart IS NULL !)");
        //    return dbcart;
        //}

        //public static DBCart SaveCart(PE_AttachToAgent Cart)
        //{
        //    Debug.Print("[Save Module] UPDATING Cart TO DB (" + Cart != null ? " " + Cart.ID : "Cart IS NULL !)");
        //    DBCart dbcart = CreateDBCart(Cart);
        //    string insertQuery = "UPDATE carts SET PosX = @PosX, PosY = @PosY, PosZ = @PosZ, InventoryId = @InventoryID WHERE ID = @ID";
        //    DBConnection.Connection.Execute(insertQuery, dbcart);
        //    Debug.Print("[Save Module] UPDATED Cart TO DB (" + Cart != null ? " " + Cart.ID : "Cart IS NULL !)");
        //    return dbcart;
        //}

    }
}
