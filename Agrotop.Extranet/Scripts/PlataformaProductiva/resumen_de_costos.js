$(document).ready(function () {
    $("#alternativas-de-rendimiento-qqm-ha-1").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_1 = ($("#alternativas-de-rendimiento-qqm-ha-1").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-1").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_1 = ($("#signo-de-pesos-qqm-1").val() != null ? $("#signo-de-pesos-qqm-1").val().replace(",", ".") : 0);
        var ingreso_bruto_1 = (alternativas_de_rendimiento_qqm_ha_1 * signo_de_pesos_qqm_1).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_1))
            ingreso_bruto_1 = 0;
        $("#ingreso-bruto-1").val(ingreso_bruto_1).trigger('change');
    });

    $("#signo-de-pesos-qqm-1").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_1 = ($("#alternativas-de-rendimiento-qqm-ha-1").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-1").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_1 = ($("#signo-de-pesos-qqm-1").val() != null ? $("#signo-de-pesos-qqm-1").val().replace(",", ".") : 0);
        var ingreso_bruto_1 = (alternativas_de_rendimiento_qqm_ha_1 * signo_de_pesos_qqm_1).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_1))
            ingreso_bruto_1 = 0;
        $("#ingreso-bruto-1").val(ingreso_bruto_1).trigger('change');
    });

    $("#ingreso-bruto-1").change(function () {
        var ingreso_bruto_1 = ($("#ingreso-bruto-1").val() != null ? $("#ingreso-bruto-1").val().replace(",", ".") : 0);
        var costos_totales_sin_arriendo = ($("#costos-totales-sin-arriendo").val() != null ? $("#costos-totales-sin-arriendo").val().replace(",", ".") : 0);
        var costos_totales_con_arriendo = ($("#costos-totales-con-arriendo").val() != null ? $("#costos-totales-con-arriendo").val().replace(",", ".") : 0);

        var ingreso_neto_sin_arriendo_1 = (ingreso_bruto_1 - costos_totales_sin_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_sin_arriendo_1))
            ingreso_neto_sin_arriendo_1 = 0;
        $("#ingreso-neto-sin-arriendo-1").val(ingreso_neto_sin_arriendo_1);

        var ingreso_neto_con_arriendo_1 = (ingreso_bruto_1 - costos_totales_con_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_con_arriendo_1))
            ingreso_neto_con_arriendo_1 = 0;
        $("#ingreso-neto-con-arriendo-1").val(ingreso_neto_con_arriendo_1);

        var ingreso_neto_sin_arriendo_porcentaje_1 = (((ingreso_bruto_1 / costos_totales_sin_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_sin_arriendo_porcentaje_1))
            ingreso_neto_sin_arriendo_porcentaje_1 = 0;
        $("#ingreso-neto-sin-arriendo-porcentaje-1").val(ingreso_neto_sin_arriendo_porcentaje_1.toFixed(2));

        var ingreso_neto_con_arriendo_porcentaje_1 = (((ingreso_bruto_1 / costos_totales_con_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_con_arriendo_porcentaje_1))
            ingreso_neto_con_arriendo_porcentaje_1 = 0;
        $("#ingreso-neto-con-arriendo-porcentaje-1").val(ingreso_neto_con_arriendo_porcentaje_1.toFixed(2));
    });

    $("#alternativas-de-rendimiento-qqm-ha-2").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_2 = ($("#alternativas-de-rendimiento-qqm-ha-2").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-2").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_2 = ($("#signo-de-pesos-qqm-2").val() != null ? $("#signo-de-pesos-qqm-2").val().replace(",", ".") : 0);
        var ingreso_bruto_2 = (alternativas_de_rendimiento_qqm_ha_2 * signo_de_pesos_qqm_2).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_2))
            ingreso_bruto_2 = 0;
        $("#ingreso-bruto-2").val(ingreso_bruto_2).trigger('change');
    });

    $("#signo-de-pesos-qqm-2").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_2 = ($("#alternativas-de-rendimiento-qqm-ha-2").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-2").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_2 = ($("#signo-de-pesos-qqm-2").val() != null ? $("#signo-de-pesos-qqm-2").val().replace(",", ".") : 0);
        var ingreso_bruto_2 = (alternativas_de_rendimiento_qqm_ha_2 * signo_de_pesos_qqm_2).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_2))
            ingreso_bruto_2 = 0;
        $("#ingreso-bruto-2").val(ingreso_bruto_2).trigger('change');
    });

    $("#ingreso-bruto-2").change(function () {
        var ingreso_bruto_2 = ($("#ingreso-bruto-2").val() != null ? $("#ingreso-bruto-2").val().replace(",", ".") : 0);
        var costos_totales_sin_arriendo = ($("#costos-totales-sin-arriendo").val() != null ? $("#costos-totales-sin-arriendo").val().replace(",", ".") : 0);
        var costos_totales_con_arriendo = ($("#costos-totales-con-arriendo").val() != null ? $("#costos-totales-con-arriendo").val().replace(",", ".") : 0);

        var ingreso_neto_sin_arriendo_2 = (ingreso_bruto_2 - costos_totales_sin_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_sin_arriendo_2))
            ingreso_neto_sin_arriendo_2 = 0;
        $("#ingreso-neto-sin-arriendo-2").val(ingreso_neto_sin_arriendo_2);

        var ingreso_neto_con_arriendo_2 = (ingreso_bruto_2 - costos_totales_con_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_con_arriendo_2))
            ingreso_neto_con_arriendo_2 = 0;
        $("#ingreso-neto-con-arriendo-2").val(ingreso_neto_con_arriendo_2);

        var ingreso_neto_sin_arriendo_porcentaje_2 = (((ingreso_bruto_2 / costos_totales_sin_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_sin_arriendo_porcentaje_2))
            ingreso_neto_sin_arriendo_porcentaje_2 = 0;
        $("#ingreso-neto-sin-arriendo-porcentaje-2").val(ingreso_neto_sin_arriendo_porcentaje_2.toFixed(2));

        var ingreso_neto_con_arriendo_porcentaje_2 = (((ingreso_bruto_2 / costos_totales_con_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_con_arriendo_porcentaje_2))
            ingreso_neto_con_arriendo_porcentaje_2 = 0;
        $("#ingreso-neto-con-arriendo-porcentaje-2").val(ingreso_neto_con_arriendo_porcentaje_2.toFixed(2));
    });

    $("#alternativas-de-rendimiento-qqm-ha-3").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_3 = ($("#alternativas-de-rendimiento-qqm-ha-3").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-3").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_3 = ($("#signo-de-pesos-qqm-3").val() != null ? $("#signo-de-pesos-qqm-3").val().replace(",", ".") : 0);
        var ingreso_bruto_3 = (alternativas_de_rendimiento_qqm_ha_3 * signo_de_pesos_qqm_3).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_3))
            ingreso_bruto_3 = 0;
        $("#ingreso-bruto-3").val(ingreso_bruto_3).trigger('change');
    });

    $("#signo-de-pesos-qqm-3").keyup(function () {
        var alternativas_de_rendimiento_qqm_ha_3 = ($("#alternativas-de-rendimiento-qqm-ha-3").val() != null ? $("#alternativas-de-rendimiento-qqm-ha-3").val().replace(",", ".") : 0);
        var signo_de_pesos_qqm_3 = ($("#signo-de-pesos-qqm-3").val() != null ? $("#signo-de-pesos-qqm-3").val().replace(",", ".") : 0);
        var ingreso_bruto_3 = (alternativas_de_rendimiento_qqm_ha_3 * signo_de_pesos_qqm_3).toString().replace(",", ".");
        if (isNaN(ingreso_bruto_3))
            ingreso_bruto_3 = 0;
        $("#ingreso-bruto-3").val(ingreso_bruto_3).trigger('change');
    });

    $("#ingreso-bruto-3").change(function () {
        var ingreso_bruto_3 = ($("#ingreso-bruto-3").val() != null ? $("#ingreso-bruto-3").val().replace(",", ".") : 0);
        var costos_totales_sin_arriendo = ($("#costos-totales-sin-arriendo").val() != null ? $("#costos-totales-sin-arriendo").val().replace(",", ".") : 0);
        var costos_totales_con_arriendo = ($("#costos-totales-con-arriendo").val() != null ? $("#costos-totales-con-arriendo").val().replace(",", ".") : 0);

        var ingreso_neto_sin_arriendo_3 = (ingreso_bruto_3 - costos_totales_sin_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_sin_arriendo_3))
            ingreso_neto_sin_arriendo_3 = 0;
        $("#ingreso-neto-sin-arriendo-3").val(ingreso_neto_sin_arriendo_3);

        var ingreso_neto_con_arriendo_3 = (ingreso_bruto_3 - costos_totales_con_arriendo).toString().replace(",", ".");
        if (isNaN(ingreso_neto_con_arriendo_3))
            ingreso_neto_con_arriendo_3 = 0;
        $("#ingreso-neto-con-arriendo-3").val(ingreso_neto_con_arriendo_3);

        var ingreso_neto_sin_arriendo_porcentaje_3 = (((ingreso_bruto_3 / costos_totales_sin_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_sin_arriendo_porcentaje_3))
            ingreso_neto_sin_arriendo_porcentaje_3 = 0;
        $("#ingreso-neto-sin-arriendo-porcentaje-3").val(ingreso_neto_sin_arriendo_porcentaje_3.toFixed(2));

        var ingreso_neto_con_arriendo_porcentaje_3 = (((ingreso_bruto_3 / costos_totales_con_arriendo) - 1) * 100);
        if (isNaN(ingreso_neto_con_arriendo_porcentaje_3))
            ingreso_neto_con_arriendo_porcentaje_3 = 0;
        $("#ingreso-neto-con-arriendo-porcentaje-3").val(ingreso_neto_con_arriendo_porcentaje_3.toFixed(2));
    });
});