SELECT * FROM Items INNER JOIN (SELECT Positions.Time as PTime, Jobs.Time as JTime, Positions.Item as PItem, Jobs.Item as JItem FROM Positions,Jobs WHERE Jobs.Item = Positions.Item) temp ON Items.Id = temp.PItem


/*SELECT * From Items INNER JOIN (SELECT * FROM Positions,Jobs WHERE Jobs.Item = Postions.Item GROUP BY Item) WWW ON Items.Id = WWW.Item*/


/*FROM Jobs INNER JOIN Items
   ON Jobs.Item = Items.Id
   
    JOIN Positions
   ON Positions.Item = Items.Id
ORDER BY ID ASC;*/