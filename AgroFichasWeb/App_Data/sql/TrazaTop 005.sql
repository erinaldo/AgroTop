USE Agrofichas
GO

ALTER TABLE Cultivo ADD
	IdForceManagerCRM INT NULL
GO

UPDATE Cultivo SET IdForceManagerCRM = 3 WHERE IdCultivo = 1		--Raps
UPDATE Cultivo SET IdForceManagerCRM = 4 WHERE IdCultivo = 2		--Trigo
UPDATE Cultivo SET IdForceManagerCRM = 1 WHERE IdCultivo = 3		--Avena
UPDATE Cultivo SET IdForceManagerCRM = 2 WHERE IdCultivo = 4		--Lupino
UPDATE Cultivo SET IdForceManagerCRM = 7 WHERE IdCultivo = 6		--Triticale
UPDATE Cultivo SET IdForceManagerCRM = 6 WHERE IdCultivo = 9999		--Otro
GO

ALTER TABLE TipoContrato ADD
	IdForceManagerCRM INT NULL
GO

UPDATE TipoContrato SET IdForceManagerCRM = 1 WHERE IdTipoContrato = 1		--Acuerdo Comercial
UPDATE TipoContrato SET IdForceManagerCRM = 2 WHERE IdTipoContrato = 2		--Contrato
UPDATE TipoContrato SET IdForceManagerCRM = 3 WHERE IdTipoContrato = 3		--Cierre de Negocio
GO

DELETE FROM TipoContrato WHERE IdTipoContrato = 4
GO

ALTER TABLE Comuna ADD
	IdForceManagerCRM INT NULL
GO

UPDATE Comuna SET IdForceManagerCRM = 1 WHERE IdComuna = 999999
UPDATE Comuna SET IdForceManagerCRM = 2 WHERE IdComuna = 11201
UPDATE Comuna SET IdForceManagerCRM = 3 WHERE IdComuna = 5602
UPDATE Comuna SET IdForceManagerCRM = 4 WHERE IdComuna = 13502
UPDATE Comuna SET IdForceManagerCRM = 5 WHERE IdComuna = 8314
UPDATE Comuna SET IdForceManagerCRM = 6 WHERE IdComuna = 3302
UPDATE Comuna SET IdForceManagerCRM = 7 WHERE IdComuna = 1107
UPDATE Comuna SET IdForceManagerCRM = 8 WHERE IdComuna = 10202
UPDATE Comuna SET IdForceManagerCRM = 9 WHERE IdComuna = 4103
UPDATE Comuna SET IdForceManagerCRM = 10 WHERE IdComuna = 9201
UPDATE Comuna SET IdForceManagerCRM = 11 WHERE IdComuna = 12202
UPDATE Comuna SET IdForceManagerCRM = 12 WHERE IdComuna = 2101
UPDATE Comuna SET IdForceManagerCRM = 13 WHERE IdComuna = 8302
UPDATE Comuna SET IdForceManagerCRM = 14 WHERE IdComuna = 8202
UPDATE Comuna SET IdForceManagerCRM = 15 WHERE IdComuna = 15101
UPDATE Comuna SET IdForceManagerCRM = 16 WHERE IdComuna = 13402
UPDATE Comuna SET IdForceManagerCRM = 17 WHERE IdComuna = 8402
UPDATE Comuna SET IdForceManagerCRM = 18 WHERE IdComuna = 5402
UPDATE Comuna SET IdForceManagerCRM = 19 WHERE IdComuna = 12201
UPDATE Comuna SET IdForceManagerCRM = 20 WHERE IdComuna = 8303
UPDATE Comuna SET IdForceManagerCRM = 21 WHERE IdComuna = 2201
UPDATE Comuna SET IdForceManagerCRM = 22 WHERE IdComuna = 10102
UPDATE Comuna SET IdForceManagerCRM = 23 WHERE IdComuna = 3102
UPDATE Comuna SET IdForceManagerCRM = 24 WHERE IdComuna = 5502
UPDATE Comuna SET IdForceManagerCRM = 25 WHERE IdComuna = 13403
UPDATE Comuna SET IdForceManagerCRM = 26 WHERE IdComuna = 5302
UPDATE Comuna SET IdForceManagerCRM = 27 WHERE IdComuna = 15102
UPDATE Comuna SET IdForceManagerCRM = 28 WHERE IdComuna = 1402
UPDATE Comuna SET IdForceManagerCRM = 29 WHERE IdComuna = 4202
UPDATE Comuna SET IdForceManagerCRM = 30 WHERE IdComuna = 8203
UPDATE Comuna SET IdForceManagerCRM = 31 WHERE IdComuna = 9102
UPDATE Comuna SET IdForceManagerCRM = 32 WHERE IdComuna = 5603
UPDATE Comuna SET IdForceManagerCRM = 33 WHERE IdComuna = 5102
UPDATE Comuna SET IdForceManagerCRM = 34 WHERE IdComuna = 10201
UPDATE Comuna SET IdForceManagerCRM = 35 WHERE IdComuna = 5702
UPDATE Comuna SET IdForceManagerCRM = 36 WHERE IdComuna = 7201
UPDATE Comuna SET IdForceManagerCRM = 37 WHERE IdComuna = 13102
UPDATE Comuna SET IdForceManagerCRM = 38 WHERE IdComuna = 13103
UPDATE Comuna SET IdForceManagerCRM = 39 WHERE IdComuna = 10401
UPDATE Comuna SET IdForceManagerCRM = 40 WHERE IdComuna = 3201
UPDATE Comuna SET IdForceManagerCRM = 41 WHERE IdComuna = 7202
UPDATE Comuna SET IdForceManagerCRM = 42 WHERE IdComuna = 6302
UPDATE Comuna SET IdForceManagerCRM = 43 WHERE IdComuna = 8103
UPDATE Comuna SET IdForceManagerCRM = 44 WHERE IdComuna = 11401
UPDATE Comuna SET IdForceManagerCRM = 45 WHERE IdComuna = 8401
UPDATE Comuna SET IdForceManagerCRM = 46 WHERE IdComuna = 8406
UPDATE Comuna SET IdForceManagerCRM = 47 WHERE IdComuna = 6303
UPDATE Comuna SET IdForceManagerCRM = 48 WHERE IdComuna = 9121
UPDATE Comuna SET IdForceManagerCRM = 49 WHERE IdComuna = 10203
UPDATE Comuna SET IdForceManagerCRM = 50 WHERE IdComuna = 11202
UPDATE Comuna SET IdForceManagerCRM = 51 WHERE IdComuna = 8403
UPDATE Comuna SET IdForceManagerCRM = 52 WHERE IdComuna = 10103
UPDATE Comuna SET IdForceManagerCRM = 53 WHERE IdComuna = 11301
UPDATE Comuna SET IdForceManagerCRM = 54 WHERE IdComuna = 6102
UPDATE Comuna SET IdForceManagerCRM = 55 WHERE IdComuna = 8404
UPDATE Comuna SET IdForceManagerCRM = 56 WHERE IdComuna = 11101
UPDATE Comuna SET IdForceManagerCRM = 57 WHERE IdComuna = 8405
UPDATE Comuna SET IdForceManagerCRM = 58 WHERE IdComuna = 6103
UPDATE Comuna SET IdForceManagerCRM = 59 WHERE IdComuna = 7402
UPDATE Comuna SET IdForceManagerCRM = 60 WHERE IdComuna = 1403
UPDATE Comuna SET IdForceManagerCRM = 61 WHERE IdComuna = 13301
UPDATE Comuna SET IdForceManagerCRM = 62 WHERE IdComuna = 9202
UPDATE Comuna SET IdForceManagerCRM = 63 WHERE IdComuna = 6104
UPDATE Comuna SET IdForceManagerCRM = 64 WHERE IdComuna = 4302
UPDATE Comuna SET IdForceManagerCRM = 65 WHERE IdComuna = 8101
UPDATE Comuna SET IdForceManagerCRM = 66 WHERE IdComuna = 13104
UPDATE Comuna SET IdForceManagerCRM = 67 WHERE IdComuna = 5103
UPDATE Comuna SET IdForceManagerCRM = 68 WHERE IdComuna = 7102
UPDATE Comuna SET IdForceManagerCRM = 69 WHERE IdComuna = 8204
UPDATE Comuna SET IdForceManagerCRM = 70 WHERE IdComuna = 3101
UPDATE Comuna SET IdForceManagerCRM = 71 WHERE IdComuna = 4102
UPDATE Comuna SET IdForceManagerCRM = 72 WHERE IdComuna = 8102
UPDATE Comuna SET IdForceManagerCRM = 73 WHERE IdComuna = 14102
UPDATE Comuna SET IdForceManagerCRM = 74 WHERE IdComuna = 9103
UPDATE Comuna SET IdForceManagerCRM = 75 WHERE IdComuna = 9203
UPDATE Comuna SET IdForceManagerCRM = 76 WHERE IdComuna = 13503
UPDATE Comuna SET IdForceManagerCRM = 77 WHERE IdComuna = 10204
UPDATE Comuna SET IdForceManagerCRM = 78 WHERE IdComuna = 8205
UPDATE Comuna SET IdForceManagerCRM = 79 WHERE IdComuna = 9104
UPDATE Comuna SET IdForceManagerCRM = 80 WHERE IdComuna = 7103
UPDATE Comuna SET IdForceManagerCRM = 81 WHERE IdComuna = 7301
UPDATE Comuna SET IdForceManagerCRM = 82 WHERE IdComuna = 10205
UPDATE Comuna SET IdForceManagerCRM = 83 WHERE IdComuna = 3202
UPDATE Comuna SET IdForceManagerCRM = 84 WHERE IdComuna = 6105
UPDATE Comuna SET IdForceManagerCRM = 85 WHERE IdComuna = 13105
UPDATE Comuna SET IdForceManagerCRM = 86 WHERE IdComuna = 8407
UPDATE Comuna SET IdForceManagerCRM = 87 WHERE IdComuna = 13602
UPDATE Comuna SET IdForceManagerCRM = 88 WHERE IdComuna = 5604
UPDATE Comuna SET IdForceManagerCRM = 89 WHERE IdComuna = 5605
UPDATE Comuna SET IdForceManagerCRM = 90 WHERE IdComuna = 7104
UPDATE Comuna SET IdForceManagerCRM = 91 WHERE IdComuna = 9204
UPDATE Comuna SET IdForceManagerCRM = 92 WHERE IdComuna = 13106
UPDATE Comuna SET IdForceManagerCRM = 93 WHERE IdComuna = 8104
UPDATE Comuna SET IdForceManagerCRM = 94 WHERE IdComuna = 9105
UPDATE Comuna SET IdForceManagerCRM = 95 WHERE IdComuna = 3303
UPDATE Comuna SET IdForceManagerCRM = 96 WHERE IdComuna = 10104
UPDATE Comuna SET IdForceManagerCRM = 97 WHERE IdComuna = 10105
UPDATE Comuna SET IdForceManagerCRM = 98 WHERE IdComuna = 10402
UPDATE Comuna SET IdForceManagerCRM = 99 WHERE IdComuna = 14202
UPDATE Comuna SET IdForceManagerCRM = 100 WHERE IdComuna = 9106
UPDATE Comuna SET IdForceManagerCRM = 101 WHERE IdComuna = 15202
UPDATE Comuna SET IdForceManagerCRM = 102 WHERE IdComuna = 9107
UPDATE Comuna SET IdForceManagerCRM = 103 WHERE IdComuna = 6106
UPDATE Comuna SET IdForceManagerCRM = 104 WHERE IdComuna = 11203
UPDATE Comuna SET IdForceManagerCRM = 105 WHERE IdComuna = 5503
UPDATE Comuna SET IdForceManagerCRM = 106 WHERE IdComuna = 10403
UPDATE Comuna SET IdForceManagerCRM = 107 WHERE IdComuna = 7302
UPDATE Comuna SET IdForceManagerCRM = 108 WHERE IdComuna = 8112
UPDATE Comuna SET IdForceManagerCRM = 109 WHERE IdComuna = 8105
UPDATE Comuna SET IdForceManagerCRM = 110 WHERE IdComuna = 1404
UPDATE Comuna SET IdForceManagerCRM = 111 WHERE IdComuna = 3304
UPDATE Comuna SET IdForceManagerCRM = 112 WHERE IdComuna = 13107
UPDATE Comuna SET IdForceManagerCRM = 113 WHERE IdComuna = 4201
UPDATE Comuna SET IdForceManagerCRM = 114 WHERE IdComuna = 13108
UPDATE Comuna SET IdForceManagerCRM = 115 WHERE IdComuna = 1101
UPDATE Comuna SET IdForceManagerCRM = 116 WHERE IdComuna = 13603
UPDATE Comuna SET IdForceManagerCRM = 117 WHERE IdComuna = 5201
UPDATE Comuna SET IdForceManagerCRM = 118 WHERE IdComuna = 5104
UPDATE Comuna SET IdForceManagerCRM = 119 WHERE IdComuna = 13109
UPDATE Comuna SET IdForceManagerCRM = 120 WHERE IdComuna = 5504
UPDATE Comuna SET IdForceManagerCRM = 121 WHERE IdComuna = 6202
UPDATE Comuna SET IdForceManagerCRM = 122 WHERE IdComuna = 13110
UPDATE Comuna SET IdForceManagerCRM = 123 WHERE IdComuna = 13111
UPDATE Comuna SET IdForceManagerCRM = 124 WHERE IdComuna = 4104
UPDATE Comuna SET IdForceManagerCRM = 125 WHERE IdComuna = 5401
UPDATE Comuna SET IdForceManagerCRM = 126 WHERE IdComuna = 13112
UPDATE Comuna SET IdForceManagerCRM = 127 WHERE IdComuna = 13113
UPDATE Comuna SET IdForceManagerCRM = 128 WHERE IdComuna = 4101
UPDATE Comuna SET IdForceManagerCRM = 129 WHERE IdComuna = 14201
UPDATE Comuna SET IdForceManagerCRM = 130 WHERE IdComuna = 9122
UPDATE Comuna SET IdForceManagerCRM = 131 WHERE IdComuna = 14203
UPDATE Comuna SET IdForceManagerCRM = 132 WHERE IdComuna = 11102
UPDATE Comuna SET IdForceManagerCRM = 133 WHERE IdComuna = 12102
UPDATE Comuna SET IdForceManagerCRM = 134 WHERE IdComuna = 8304
UPDATE Comuna SET IdForceManagerCRM = 135 WHERE IdComuna = 13302
UPDATE Comuna SET IdForceManagerCRM = 136 WHERE IdComuna = 14103
UPDATE Comuna SET IdForceManagerCRM = 137 WHERE IdComuna = 6107
UPDATE Comuna SET IdForceManagerCRM = 138 WHERE IdComuna = 13114
UPDATE Comuna SET IdForceManagerCRM = 139 WHERE IdComuna = 9108
UPDATE Comuna SET IdForceManagerCRM = 140 WHERE IdComuna = 8201
UPDATE Comuna SET IdForceManagerCRM = 141 WHERE IdComuna = 7303
UPDATE Comuna SET IdForceManagerCRM = 142 WHERE IdComuna = 5802
UPDATE Comuna SET IdForceManagerCRM = 143 WHERE IdComuna = 7401
UPDATE Comuna SET IdForceManagerCRM = 144 WHERE IdComuna = 6203
UPDATE Comuna SET IdForceManagerCRM = 145 WHERE IdComuna = 5703
UPDATE Comuna SET IdForceManagerCRM = 146 WHERE IdComuna = 10107
UPDATE Comuna SET IdForceManagerCRM = 147 WHERE IdComuna = 13115
UPDATE Comuna SET IdForceManagerCRM = 148 WHERE IdComuna = 13116
UPDATE Comuna SET IdForceManagerCRM = 149 WHERE IdComuna = 13117
UPDATE Comuna SET IdForceManagerCRM = 150 WHERE IdComuna = 6304
UPDATE Comuna SET IdForceManagerCRM = 151 WHERE IdComuna = 9109
UPDATE Comuna SET IdForceManagerCRM = 152 WHERE IdComuna = 7403
UPDATE Comuna SET IdForceManagerCRM = 153 WHERE IdComuna = 9205
UPDATE Comuna SET IdForceManagerCRM = 154 WHERE IdComuna = 8206
UPDATE Comuna SET IdForceManagerCRM = 155 WHERE IdComuna = 5301
UPDATE Comuna SET IdForceManagerCRM = 156 WHERE IdComuna = 8301
UPDATE Comuna SET IdForceManagerCRM = 157 WHERE IdComuna = 14104
UPDATE Comuna SET IdForceManagerCRM = 158 WHERE IdComuna = 10106
UPDATE Comuna SET IdForceManagerCRM = 159 WHERE IdComuna = 9206
UPDATE Comuna SET IdForceManagerCRM = 160 WHERE IdComuna = 4203
UPDATE Comuna SET IdForceManagerCRM = 161 WHERE IdComuna = 8106
UPDATE Comuna SET IdForceManagerCRM = 162 WHERE IdComuna = 9207
UPDATE Comuna SET IdForceManagerCRM = 163 WHERE IdComuna = 6108
UPDATE Comuna SET IdForceManagerCRM = 164 WHERE IdComuna = 13118
UPDATE Comuna SET IdForceManagerCRM = 165 WHERE IdComuna = 14105
UPDATE Comuna SET IdForceManagerCRM = 166 WHERE IdComuna = 13119
UPDATE Comuna SET IdForceManagerCRM = 167 WHERE IdComuna = 6109
UPDATE Comuna SET IdForceManagerCRM = 168 WHERE IdComuna = 6204
UPDATE Comuna SET IdForceManagerCRM = 169 WHERE IdComuna = 2302
UPDATE Comuna SET IdForceManagerCRM = 170 WHERE IdComuna = 13504
UPDATE Comuna SET IdForceManagerCRM = 171 WHERE IdComuna = 14106
UPDATE Comuna SET IdForceManagerCRM = 172 WHERE IdComuna = 7105
UPDATE Comuna SET IdForceManagerCRM = 173 WHERE IdComuna = 10108
UPDATE Comuna SET IdForceManagerCRM = 174 WHERE IdComuna = 2102
UPDATE Comuna SET IdForceManagerCRM = 175 WHERE IdComuna = 9110
UPDATE Comuna SET IdForceManagerCRM = 176 WHERE IdComuna = 13501
UPDATE Comuna SET IdForceManagerCRM = 177 WHERE IdComuna = 7304
UPDATE Comuna SET IdForceManagerCRM = 178 WHERE IdComuna = 4303
UPDATE Comuna SET IdForceManagerCRM = 179 WHERE IdComuna = 6110
UPDATE Comuna SET IdForceManagerCRM = 180 WHERE IdComuna = 8305
UPDATE Comuna SET IdForceManagerCRM = 181 WHERE IdComuna = 8306
UPDATE Comuna SET IdForceManagerCRM = 182 WHERE IdComuna = 6305
UPDATE Comuna SET IdForceManagerCRM = 183 WHERE IdComuna = 12401
UPDATE Comuna SET IdForceManagerCRM = 184 WHERE IdComuna = 6205
UPDATE Comuna SET IdForceManagerCRM = 185 WHERE IdComuna = 8307
UPDATE Comuna SET IdForceManagerCRM = 186 WHERE IdComuna = 8408
UPDATE Comuna SET IdForceManagerCRM = 187 WHERE IdComuna = 8409
UPDATE Comuna SET IdForceManagerCRM = 188 WHERE IdComuna = 5506
UPDATE Comuna SET IdForceManagerCRM = 189 WHERE IdComuna = 9111
UPDATE Comuna SET IdForceManagerCRM = 190 WHERE IdComuna = 13120
UPDATE Comuna SET IdForceManagerCRM = 191 WHERE IdComuna = 11302
UPDATE Comuna SET IdForceManagerCRM = 192 WHERE IdComuna = 6111
UPDATE Comuna SET IdForceManagerCRM = 193 WHERE IdComuna = 2202
UPDATE Comuna SET IdForceManagerCRM = 194 WHERE IdComuna = 5803
UPDATE Comuna SET IdForceManagerCRM = 195 WHERE IdComuna = 10301
UPDATE Comuna SET IdForceManagerCRM = 196 WHERE IdComuna = 4301
UPDATE Comuna SET IdForceManagerCRM = 197 WHERE IdComuna = 13604
UPDATE Comuna SET IdForceManagerCRM = 198 WHERE IdComuna = 9112
UPDATE Comuna SET IdForceManagerCRM = 199 WHERE IdComuna = 4105
UPDATE Comuna SET IdForceManagerCRM = 200 WHERE IdComuna = 14107
UPDATE Comuna SET IdForceManagerCRM = 201 WHERE IdComuna = 13404
UPDATE Comuna SET IdForceManagerCRM = 202 WHERE IdComuna = 10404
UPDATE Comuna SET IdForceManagerCRM = 203 WHERE IdComuna = 6306
UPDATE Comuna SET IdForceManagerCRM = 204 WHERE IdComuna = 14108
UPDATE Comuna SET IdForceManagerCRM = 205 WHERE IdComuna = 5704
UPDATE Comuna SET IdForceManagerCRM = 206 WHERE IdComuna = 5403
UPDATE Comuna SET IdForceManagerCRM = 207 WHERE IdComuna = 6206
UPDATE Comuna SET IdForceManagerCRM = 208 WHERE IdComuna = 7404
UPDATE Comuna SET IdForceManagerCRM = 209 WHERE IdComuna = 13121
UPDATE Comuna SET IdForceManagerCRM = 210 WHERE IdComuna = 7106
UPDATE Comuna SET IdForceManagerCRM = 211 WHERE IdComuna = 7203
UPDATE Comuna SET IdForceManagerCRM = 212 WHERE IdComuna = 8410
UPDATE Comuna SET IdForceManagerCRM = 213 WHERE IdComuna = 13605
UPDATE Comuna SET IdForceManagerCRM = 214 WHERE IdComuna = 13122
UPDATE Comuna SET IdForceManagerCRM = 215 WHERE IdComuna = 7107
UPDATE Comuna SET IdForceManagerCRM = 216 WHERE IdComuna = 8107
UPDATE Comuna SET IdForceManagerCRM = 217 WHERE IdComuna = 6307
UPDATE Comuna SET IdForceManagerCRM = 218 WHERE IdComuna = 9113
UPDATE Comuna SET IdForceManagerCRM = 219 WHERE IdComuna = 5404
UPDATE Comuna SET IdForceManagerCRM = 220 WHERE IdComuna = 6112
UPDATE Comuna SET IdForceManagerCRM = 221 WHERE IdComuna = 1405
UPDATE Comuna SET IdForceManagerCRM = 222 WHERE IdComuna = 6113
UPDATE Comuna SET IdForceManagerCRM = 223 WHERE IdComuna = 6201
UPDATE Comuna SET IdForceManagerCRM = 224 WHERE IdComuna = 8411
UPDATE Comuna SET IdForceManagerCRM = 225 WHERE IdComuna = 13202
UPDATE Comuna SET IdForceManagerCRM = 226 WHERE IdComuna = 9114
UPDATE Comuna SET IdForceManagerCRM = 227 WHERE IdComuna = 6308
UPDATE Comuna SET IdForceManagerCRM = 228 WHERE IdComuna = 8412
UPDATE Comuna SET IdForceManagerCRM = 229 WHERE IdComuna = 12301
UPDATE Comuna SET IdForceManagerCRM = 230 WHERE IdComuna = 1401
UPDATE Comuna SET IdForceManagerCRM = 231 WHERE IdComuna = 12302
UPDATE Comuna SET IdForceManagerCRM = 232 WHERE IdComuna = 13123
UPDATE Comuna SET IdForceManagerCRM = 233 WHERE IdComuna = 5105
UPDATE Comuna SET IdForceManagerCRM = 234 WHERE IdComuna = 9115
UPDATE Comuna SET IdForceManagerCRM = 235 WHERE IdComuna = 13124
UPDATE Comuna SET IdForceManagerCRM = 236 WHERE IdComuna = 13201
UPDATE Comuna SET IdForceManagerCRM = 237 WHERE IdComuna = 10101
UPDATE Comuna SET IdForceManagerCRM = 238 WHERE IdComuna = 10302
UPDATE Comuna SET IdForceManagerCRM = 239 WHERE IdComuna = 10109
UPDATE Comuna SET IdForceManagerCRM = 240 WHERE IdComuna = 6309
UPDATE Comuna SET IdForceManagerCRM = 241 WHERE IdComuna = 4304
UPDATE Comuna SET IdForceManagerCRM = 242 WHERE IdComuna = 12101
UPDATE Comuna SET IdForceManagerCRM = 243 WHERE IdComuna = 10206
UPDATE Comuna SET IdForceManagerCRM = 244 WHERE IdComuna = 9208
UPDATE Comuna SET IdForceManagerCRM = 245 WHERE IdComuna = 10303
UPDATE Comuna SET IdForceManagerCRM = 246 WHERE IdComuna = 5705
UPDATE Comuna SET IdForceManagerCRM = 247 WHERE IdComuna = 15201
UPDATE Comuna SET IdForceManagerCRM = 248 WHERE IdComuna = 10304
UPDATE Comuna SET IdForceManagerCRM = 249 WHERE IdComuna = 10207
UPDATE Comuna SET IdForceManagerCRM = 250 WHERE IdComuna = 10208
UPDATE Comuna SET IdForceManagerCRM = 251 WHERE IdComuna = 10209
UPDATE Comuna SET IdForceManagerCRM = 252 WHERE IdComuna = 8308
UPDATE Comuna SET IdForceManagerCRM = 253 WHERE IdComuna = 13125
UPDATE Comuna SET IdForceManagerCRM = 254 WHERE IdComuna = 8309
UPDATE Comuna SET IdForceManagerCRM = 255 WHERE IdComuna = 8413
UPDATE Comuna SET IdForceManagerCRM = 256 WHERE IdComuna = 5501
UPDATE Comuna SET IdForceManagerCRM = 257 WHERE IdComuna = 5801
UPDATE Comuna SET IdForceManagerCRM = 258 WHERE IdComuna = 10210
UPDATE Comuna SET IdForceManagerCRM = 259 WHERE IdComuna = 6114
UPDATE Comuna SET IdForceManagerCRM = 260 WHERE IdComuna = 13126
UPDATE Comuna SET IdForceManagerCRM = 261 WHERE IdComuna = 5107
UPDATE Comuna SET IdForceManagerCRM = 262 WHERE IdComuna = 8414
UPDATE Comuna SET IdForceManagerCRM = 263 WHERE IdComuna = 6101
UPDATE Comuna SET IdForceManagerCRM = 264 WHERE IdComuna = 8415
UPDATE Comuna SET IdForceManagerCRM = 265 WHERE IdComuna = 7305
UPDATE Comuna SET IdForceManagerCRM = 266 WHERE IdComuna = 13127
UPDATE Comuna SET IdForceManagerCRM = 267 WHERE IdComuna = 9209
UPDATE Comuna SET IdForceManagerCRM = 268 WHERE IdComuna = 13128
UPDATE Comuna SET IdForceManagerCRM = 269 WHERE IdComuna = 6115
UPDATE Comuna SET IdForceManagerCRM = 270 WHERE IdComuna = 6116
UPDATE Comuna SET IdForceManagerCRM = 271 WHERE IdComuna = 7405
UPDATE Comuna SET IdForceManagerCRM = 272 WHERE IdComuna = 5303
UPDATE Comuna SET IdForceManagerCRM = 273 WHERE IdComuna = 14204
UPDATE Comuna SET IdForceManagerCRM = 274 WHERE IdComuna = 7108
UPDATE Comuna SET IdForceManagerCRM = 275 WHERE IdComuna = 4305
UPDATE Comuna SET IdForceManagerCRM = 276 WHERE IdComuna = 11402
UPDATE Comuna SET IdForceManagerCRM = 277 WHERE IdComuna = 10305
UPDATE Comuna SET IdForceManagerCRM = 278 WHERE IdComuna = 12103
UPDATE Comuna SET IdForceManagerCRM = 279 WHERE IdComuna = 7306
UPDATE Comuna SET IdForceManagerCRM = 280 WHERE IdComuna = 9116
UPDATE Comuna SET IdForceManagerCRM = 281 WHERE IdComuna = 7307
UPDATE Comuna SET IdForceManagerCRM = 282 WHERE IdComuna = 4204
UPDATE Comuna SET IdForceManagerCRM = 283 WHERE IdComuna = 5601
UPDATE Comuna SET IdForceManagerCRM = 284 WHERE IdComuna = 13401
UPDATE Comuna SET IdForceManagerCRM = 285 WHERE IdComuna = 8416
UPDATE Comuna SET IdForceManagerCRM = 286 WHERE IdComuna = 7109
UPDATE Comuna SET IdForceManagerCRM = 287 WHERE IdComuna = 5304
UPDATE Comuna SET IdForceManagerCRM = 288 WHERE IdComuna = 8417
UPDATE Comuna SET IdForceManagerCRM = 289 WHERE IdComuna = 5701
UPDATE Comuna SET IdForceManagerCRM = 290 WHERE IdComuna = 6301
UPDATE Comuna SET IdForceManagerCRM = 291 WHERE IdComuna = 12104
UPDATE Comuna SET IdForceManagerCRM = 292 WHERE IdComuna = 8418
UPDATE Comuna SET IdForceManagerCRM = 293 WHERE IdComuna = 7406
UPDATE Comuna SET IdForceManagerCRM = 294 WHERE IdComuna = 13129
UPDATE Comuna SET IdForceManagerCRM = 295 WHERE IdComuna = 13203
UPDATE Comuna SET IdForceManagerCRM = 296 WHERE IdComuna = 10306
UPDATE Comuna SET IdForceManagerCRM = 297 WHERE IdComuna = 13130
UPDATE Comuna SET IdForceManagerCRM = 298 WHERE IdComuna = 8419
UPDATE Comuna SET IdForceManagerCRM = 299 WHERE IdComuna = 10307
UPDATE Comuna SET IdForceManagerCRM = 300 WHERE IdComuna = 13505
UPDATE Comuna SET IdForceManagerCRM = 301 WHERE IdComuna = 2203
UPDATE Comuna SET IdForceManagerCRM = 302 WHERE IdComuna = 8108
UPDATE Comuna SET IdForceManagerCRM = 303 WHERE IdComuna = 7110
UPDATE Comuna SET IdForceManagerCRM = 304 WHERE IdComuna = 13131
UPDATE Comuna SET IdForceManagerCRM = 305 WHERE IdComuna = 8310
UPDATE Comuna SET IdForceManagerCRM = 306 WHERE IdComuna = 6117
UPDATE Comuna SET IdForceManagerCRM = 307 WHERE IdComuna = 8311
UPDATE Comuna SET IdForceManagerCRM = 308 WHERE IdComuna = 6310
UPDATE Comuna SET IdForceManagerCRM = 309 WHERE IdComuna = 8109
UPDATE Comuna SET IdForceManagerCRM = 310 WHERE IdComuna = 5706
UPDATE Comuna SET IdForceManagerCRM = 311 WHERE IdComuna = 13101
UPDATE Comuna SET IdForceManagerCRM = 312 WHERE IdComuna = 5606
UPDATE Comuna SET IdForceManagerCRM = 313 WHERE IdComuna = 2103
UPDATE Comuna SET IdForceManagerCRM = 314 WHERE IdComuna = 13601
UPDATE Comuna SET IdForceManagerCRM = 315 WHERE IdComuna = 7101
UPDATE Comuna SET IdForceManagerCRM = 316 WHERE IdComuna = 8110
UPDATE Comuna SET IdForceManagerCRM = 317 WHERE IdComuna = 2104
UPDATE Comuna SET IdForceManagerCRM = 318 WHERE IdComuna = 9101
UPDATE Comuna SET IdForceManagerCRM = 319 WHERE IdComuna = 7308
UPDATE Comuna SET IdForceManagerCRM = 320 WHERE IdComuna = 9117
UPDATE Comuna SET IdForceManagerCRM = 321 WHERE IdComuna = 3103
UPDATE Comuna SET IdForceManagerCRM = 322 WHERE IdComuna = 13303
UPDATE Comuna SET IdForceManagerCRM = 323 WHERE IdComuna = 12303
UPDATE Comuna SET IdForceManagerCRM = 324 WHERE IdComuna = 8207
UPDATE Comuna SET IdForceManagerCRM = 325 WHERE IdComuna = 2301
UPDATE Comuna SET IdForceManagerCRM = 326 WHERE IdComuna = 9118
UPDATE Comuna SET IdForceManagerCRM = 327 WHERE IdComuna = 8111
UPDATE Comuna SET IdForceManagerCRM = 328 WHERE IdComuna = 12402
UPDATE Comuna SET IdForceManagerCRM = 329 WHERE IdComuna = 11303
UPDATE Comuna SET IdForceManagerCRM = 330 WHERE IdComuna = 9210
UPDATE Comuna SET IdForceManagerCRM = 331 WHERE IdComuna = 8420
UPDATE Comuna SET IdForceManagerCRM = 332 WHERE IdComuna = 8312
UPDATE Comuna SET IdForceManagerCRM = 333 WHERE IdComuna = 14101
UPDATE Comuna SET IdForceManagerCRM = 334 WHERE IdComuna = 3301
UPDATE Comuna SET IdForceManagerCRM = 335 WHERE IdComuna = 5101
UPDATE Comuna SET IdForceManagerCRM = 336 WHERE IdComuna = 7309
UPDATE Comuna SET IdForceManagerCRM = 337 WHERE IdComuna = 9211
UPDATE Comuna SET IdForceManagerCRM = 338 WHERE IdComuna = 4106
UPDATE Comuna SET IdForceManagerCRM = 339 WHERE IdComuna = 9119
UPDATE Comuna SET IdForceManagerCRM = 340 WHERE IdComuna = 7407
UPDATE Comuna SET IdForceManagerCRM = 341 WHERE IdComuna = 5804
UPDATE Comuna SET IdForceManagerCRM = 342 WHERE IdComuna = 9120
UPDATE Comuna SET IdForceManagerCRM = 343 WHERE IdComuna = 5109
UPDATE Comuna SET IdForceManagerCRM = 344 WHERE IdComuna = 13132
UPDATE Comuna SET IdForceManagerCRM = 345 WHERE IdComuna = 7408
UPDATE Comuna SET IdForceManagerCRM = 346 WHERE IdComuna = 8313
UPDATE Comuna SET IdForceManagerCRM = 347 WHERE IdComuna = 8421
UPDATE Comuna SET IdForceManagerCRM = 348 WHERE IdComuna = 5405
GO

ALTER TABLE Sucursal ADD
	IdForceManagerCRM INT NULL
GO

UPDATE Sucursal SET IdForceManagerCRM = 9 WHERE IdSucursal = 1	--Planta Freire
UPDATE Sucursal SET IdForceManagerCRM = 10 WHERE IdSucursal = 2	--Planta Río Bueno
UPDATE Sucursal SET IdForceManagerCRM = 12 WHERE IdSucursal = 3	--Rucapequen
UPDATE Sucursal SET IdForceManagerCRM = 11 WHERE IdSucursal = 4	--Pueblo Seco
UPDATE Sucursal SET IdForceManagerCRM = 8 WHERE IdSucursal = 5	--Metrenco
UPDATE Sucursal SET IdForceManagerCRM = 13 WHERE IdSucursal = 6	--Victoria
UPDATE Sucursal SET IdForceManagerCRM = 2 WHERE IdSucursal = 7	--Externa
UPDATE Sucursal SET IdForceManagerCRM = 7 WHERE IdSucursal = 8	--Los Ángeles - Cotrisa
UPDATE Sucursal SET IdForceManagerCRM = 3 WHERE IdSucursal = 9	--Fusión
UPDATE Sucursal SET IdForceManagerCRM = 5 WHERE IdSucursal = 10	--Lautaro - Copeval
UPDATE Sucursal SET IdForceManagerCRM = 14 WHERE IdSucursal = 11	--Victoria - Vitra
UPDATE Sucursal SET IdForceManagerCRM = 4 WHERE IdSucursal = 12	--Lautaro - Agrotop
UPDATE Sucursal SET IdForceManagerCRM = 6 WHERE IdSucursal = 13	--Los Ángeles - Copeval
UPDATE Sucursal SET IdForceManagerCRM = 1 WHERE IdSucursal = 14	--Alisur
GO

ALTER TABLE CultivoContrato ADD
	IdForceManagerCRM INT NULL
GO

DROP TABLE [SolicitudContratoVariedad]
GO

DROP TABLE [SolicitudContrato]
GO

CREATE TABLE [dbo].[SolicitudContrato](
	[IdSolicitudContrato] [int] NOT NULL,
	[Rut] [varchar](50) NOT NULL,
	[NombreProveedor] [varchar](250) NOT NULL,
	[IdCultivo] [int] NULL,
	[Cultivo] [varchar](50) NOT NULL,
	[PrecioCierre] [int] NOT NULL,
	[ToneladasCierre] [int] NOT NULL,
	[IdTipoContrato] [int] NULL,
	[TipoContrato] [varchar](50) NOT NULL,
	[IdComunaOrigen] [int] NULL,
	[ComunaOrigen] [varchar](50) NOT NULL,
	[IdSucursalEntrega] [int] NULL,
	[SucursalEntrega] [varchar](50) NOT NULL,
	[Hectareas] [int] NOT NULL,
	[ToneladasTotales] [int] NOT NULL,
	[Predio] [varchar](50) NOT NULL,
	[VerificadoCRM] [bit] NOT NULL,
	[VerificadoFichas] [bit] NOT NULL,
	[Verificado]  AS (CONVERT([bit],case when [VerificadoCRM]=(1) AND [VerificadoFichas]=(1) then (1) else (0) end)),
	[NecesitaAutorizacion]  AS (CONVERT([bit],case when [PrecioCierre]>(0) AND [ToneladasCierre]>(0) then (1) else (0) end)),
	[Autorizado] [bit] NOT NULL,
	[UserAutorizadoIns] [varchar](50) NULL,
	[FechaHoraAutorizadoIns] [datetime] NULL,
	[IpAutorizadoIns] [varchar](50) NULL,
	[ContratoCreado] [bit] NOT NULL,
	[CierreCreado] [bit] NOT NULL,
	[IdTemporada] [int] NULL,
	[Temporada] [varchar](50) NOT NULL,
	[IdEmpresa] [int] NULL,
	[IdAgricultor] [int] NULL,
	[IdContrato] [int] NULL,
	[IdEstado] [int] NOT NULL,
	[NombreAsesor] [varchar](250) NOT NULL,
	[EmailAsesor] [varchar](250) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
	CONSTRAINT [PK_SolicitudContrato] PRIMARY KEY ([IdSolicitudContrato]),
	CONSTRAINT [FK_SolicitudContrato_Cultivo] FOREIGN KEY([IdCultivo]) REFERENCES [dbo].[Cultivo] ([IdCultivo]),
	CONSTRAINT [FK_SolicitudContrato_TipoContrato] FOREIGN KEY([IdTipoContrato]) REFERENCES [dbo].[TipoContrato] ([IdTipoContrato]),
	CONSTRAINT [FK_SolicitudContrato_Comuna] FOREIGN KEY([IdComunaOrigen]) REFERENCES [dbo].[Comuna] ([IdComuna]),
	CONSTRAINT [FK_SolicitudContrato_Sucursal] FOREIGN KEY([IdSucursalEntrega]) REFERENCES [dbo].[Sucursal] ([IdSucursal]),
	CONSTRAINT [FK_SolicitudContrato_Temporada] FOREIGN KEY([IdTemporada]) REFERENCES [dbo].[Temporada] ([IdTemporada]),
	CONSTRAINT [FK_SolicitudContrato_Empresa] FOREIGN KEY([IdEmpresa]) REFERENCES [dbo].[Empresa] ([IdEmpresa]),
	CONSTRAINT [FK_SolicitudContrato_Agricultor] FOREIGN KEY([IdAgricultor]) REFERENCES [dbo].[Agricultor] ([IdAgricultor]),
	CONSTRAINT [FK_SolicitudContrato_Contrato] FOREIGN KEY([IdContrato]) REFERENCES [dbo].[Contrato] ([IdContrato]),
	CONSTRAINT [FK_SolicitudContrato_EstadoSolicitudContrato] FOREIGN KEY([IdEstado]) REFERENCES [dbo].[EstadoSolicitudContrato] ([IdEstado])
)
GO

CREATE TABLE [SolicitudContratoVariedad]
(
	[IdSolicitudContrato] [int] NOT NULL,
	[IdVariedad] [int] NOT NULL,
	[Variedad] [varchar](50) NOT NULL,
	[Hectareas] [int] NOT NULL,
	[Toneladas] [int] NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
	CONSTRAINT [PK_SolicitudContratoVariedad] PRIMARY KEY ([IdSolicitudContrato],[IdVariedad]),
	CONSTRAINT [FK_SolicitudContratoVariedad_SolicitudContrato] FOREIGN KEY ([IdSolicitudContrato]) REFERENCES [SolicitudContrato],
	CONSTRAINT [FK_SolicitudContratoVariedad_CultivoContrato] FOREIGN KEY([IdVariedad]) REFERENCES [dbo].[CultivoContrato] ([IdCultivoContrato])
)
GO

INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Jupiter INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Neptuno INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Pincoya Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Supernova INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Armony Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Wombat Isopro', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 3, 'Avena Urano INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 4, 'Lupino Alboroto', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 4, 'Lupino Victor Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 4, 'Lupino Australiano', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 4, 'Lupino Rumbo', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Dalton Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Dozzen CL Agroas', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Dozzen Agroas', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Exclusiv CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Expertise CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Hyola CL Isopro', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Imageos CL CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Imminent CL CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Implement CL CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Imaret CIS', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Phoenix CL Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Trust CL Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Veritas CL Anasac', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Dax', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Dominator', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Nizza', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Himalaya', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Bender', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Humberto', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 1, 'Raps Clavier', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Bakán Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Bicentenario INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Buenno Syngenta', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Crac Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Dollinco INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Fritz Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Instinct Syngenta', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Invento Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Konde INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Kiron INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Kumpa INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Llareta INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Lleuque INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Maxi Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Maxwell INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Miradoux Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Orvantis Syngenta', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Otto Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Pantera INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Pandora INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Rocky INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Matylda Anasac', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Patras Anasac', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Tovak Ingentec', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Apertus Ingentec', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Galant SYNGENTA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Ilustre Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Kintus Ingentec', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Pionero INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 2, 'Trigo Ñeque Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 9999, 'Otro Cebada cervecera', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 6, 'Triticale Aguacero INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 6, 'Triticale Faraon Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 6, 'Triticale Emperador INIA', 'jfersep', GETDATE(), '127.0.0.1', NULL)
INSERT INTO [dbo].[CultivoContrato] ([IdCultivoContrato],[IdCultivo],[Nombre],[UserIns],[FechaHoraIns],[IpIns],[IdForceManagerCRM]) VALUES ((SELECT (MAX(IdCultivoContrato)+1) FROM CultivoContrato), 6, 'Triticale Torete Baer', 'jfersep', GETDATE(), '127.0.0.1', NULL)
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 7 WHERE IdCultivoContrato = 13	--Avena Akina
UPDATE CultivoContrato SET IdForceManagerCRM = 6 WHERE IdCultivoContrato = 18	--Avena Armony Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 1 WHERE IdCultivoContrato = 14	--Avena Jupiter INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 2 WHERE IdCultivoContrato = 15	--Avena Neptuno INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 3 WHERE IdCultivoContrato = 16	--Avena Pincoya Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 4 WHERE IdCultivoContrato = 17	--Avena Supernova INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 5 WHERE IdCultivoContrato = 4	--Avena Symphony
UPDATE CultivoContrato SET IdForceManagerCRM = 9 WHERE IdCultivoContrato = 20	--Avena Urano INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 8 WHERE IdCultivoContrato = 19	--Avena Wombat Isopro
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 10 WHERE IdCultivoContrato = 21	--Lupino Alboroto
UPDATE CultivoContrato SET IdForceManagerCRM = 12 WHERE IdCultivoContrato = 23	--Lupino Australiano
UPDATE CultivoContrato SET IdForceManagerCRM = 13 WHERE IdCultivoContrato = 24	--Lupino Rumbo
UPDATE CultivoContrato SET IdForceManagerCRM = 11 WHERE IdCultivoContrato = 22	--Lupino Victor Baer
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 32 WHERE IdCultivoContrato = 42	--Raps Bender
UPDATE CultivoContrato SET IdForceManagerCRM = 34 WHERE IdCultivoContrato = 44	--Raps Clavier
UPDATE CultivoContrato SET IdForceManagerCRM = 15 WHERE IdCultivoContrato = 25	--Raps Dalton Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 28 WHERE IdCultivoContrato = 38	--Raps Dax
UPDATE CultivoContrato SET IdForceManagerCRM = 29 WHERE IdCultivoContrato = 39	--Raps Dominator
UPDATE CultivoContrato SET IdForceManagerCRM = 17 WHERE IdCultivoContrato = 27	--Raps Dozzen Agroas
UPDATE CultivoContrato SET IdForceManagerCRM = 16 WHERE IdCultivoContrato = 26	--Raps Dozzen CL Agroas
UPDATE CultivoContrato SET IdForceManagerCRM = 18 WHERE IdCultivoContrato = 28	--Raps Exclusiv CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 19 WHERE IdCultivoContrato = 29	--Raps Expertise CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 31 WHERE IdCultivoContrato = 41	--Raps Himalaya
UPDATE CultivoContrato SET IdForceManagerCRM = 33 WHERE IdCultivoContrato = 43	--Raps Humberto
UPDATE CultivoContrato SET IdForceManagerCRM = 20 WHERE IdCultivoContrato = 30	--Raps Hyola CL Isopro
UPDATE CultivoContrato SET IdForceManagerCRM = 21 WHERE IdCultivoContrato = 31	--Raps Imageos CL CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 24 WHERE IdCultivoContrato = 34	--Raps Imaret CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 22 WHERE IdCultivoContrato = 32	--Raps Imminent CL CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 23 WHERE IdCultivoContrato = 33	--Raps Implement CL CIS
UPDATE CultivoContrato SET IdForceManagerCRM = 30 WHERE IdCultivoContrato = 40	--Raps Nizza
UPDATE CultivoContrato SET IdForceManagerCRM = 25 WHERE IdCultivoContrato = 35	--Raps Phoenix CL Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 26 WHERE IdCultivoContrato = 36	--Raps Trust CL Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 27 WHERE IdCultivoContrato = 37	--Raps Veritas CL Anasac
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 38 WHERE IdCultivoContrato = 5	--Trigo Caluga
UPDATE CultivoContrato SET IdForceManagerCRM = 35 WHERE IdCultivoContrato = 45	--Trigo Bakán Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 36 WHERE IdCultivoContrato = 46	--Trigo Bicentenario INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 37 WHERE IdCultivoContrato = 47	--Trigo Buenno Syngenta
UPDATE CultivoContrato SET IdForceManagerCRM = 39 WHERE IdCultivoContrato = 48	--Trigo Crac Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 40 WHERE IdCultivoContrato = 49	--Trigo Dollinco INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 41 WHERE IdCultivoContrato = 50	--Trigo Fritz Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 42 WHERE IdCultivoContrato = 51	--Trigo Instinct Syngenta
UPDATE CultivoContrato SET IdForceManagerCRM = 43 WHERE IdCultivoContrato = 52	--Trigo Invento Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 44 WHERE IdCultivoContrato = 53	--Trigo Konde INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 45 WHERE IdCultivoContrato = 54	--Trigo Kiron INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 46 WHERE IdCultivoContrato = 55	--Trigo Kumpa INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 47 WHERE IdCultivoContrato = 56	--Trigo Llareta INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 48 WHERE IdCultivoContrato = 57	--Trigo Lleuque INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 49 WHERE IdCultivoContrato = 58	--Trigo Maxi Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 50 WHERE IdCultivoContrato = 59	--Trigo Maxwell INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 51 WHERE IdCultivoContrato = 60	--Trigo Miradoux Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 52 WHERE IdCultivoContrato = 61	--Trigo Orvantis Syngenta
UPDATE CultivoContrato SET IdForceManagerCRM = 53 WHERE IdCultivoContrato = 62	--Trigo Otto Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 54 WHERE IdCultivoContrato = 63	--Trigo Pantera INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 55 WHERE IdCultivoContrato = 64	--Trigo Pandora INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 56 WHERE IdCultivoContrato = 65	--Trigo Rocky INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 57 WHERE IdCultivoContrato = 66	--Trigo Matylda Anasac
UPDATE CultivoContrato SET IdForceManagerCRM = 58 WHERE IdCultivoContrato = 67	--Trigo Patras Anasac
UPDATE CultivoContrato SET IdForceManagerCRM = 59 WHERE IdCultivoContrato = 68	--Trigo Tovak Ingentec
UPDATE CultivoContrato SET IdForceManagerCRM = 60 WHERE IdCultivoContrato = 69	--Trigo Apertus Ingentec
UPDATE CultivoContrato SET IdForceManagerCRM = 61 WHERE IdCultivoContrato = 70	--Trigo Galant SYNGENTA
UPDATE CultivoContrato SET IdForceManagerCRM = 62 WHERE IdCultivoContrato = 71	--Trigo Ilustre Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 63 WHERE IdCultivoContrato = 72	--Trigo Kintus Ingentec
UPDATE CultivoContrato SET IdForceManagerCRM = 64 WHERE IdCultivoContrato = 73	--Trigo Pionero INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 65 WHERE IdCultivoContrato = 74	--Trigo Ñeque Baer
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 14 WHERE IdCultivoContrato = 75	--Otro Cebada cervecera
GO

UPDATE CultivoContrato SET IdForceManagerCRM = 66 WHERE IdCultivoContrato = 76	--Triticale Aguacero INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 67 WHERE IdCultivoContrato = 77	--Triticale Faraon Baer
UPDATE CultivoContrato SET IdForceManagerCRM = 68 WHERE IdCultivoContrato = 78	--Triticale Emperador INIA
UPDATE CultivoContrato SET IdForceManagerCRM = 69 WHERE IdCultivoContrato = 79	--Triticale Torete Baer
GO