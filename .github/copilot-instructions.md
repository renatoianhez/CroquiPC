# Copilot Instructions

## Diretrizes de projeto
- Para templates de trevo, preferir carregar arquivos .csk reais da pasta Templates em vez de fabricar a geometria em código.
- No AutoIntersectionObject, a sinalização (PARE/faixa de pedestres) deve considerar as duas vias independentemente da seleção de Parte (R1/R2), quando essa for a expectativa funcional.
- No AutoIntersectionObject, o painel de propriedades deve exibir apenas as propriedades da parte selecionada (R1/R2), mantendo comuns quando aplicável.

## Instruções de Design
- Para a pose `DeLado` em `StickFigure`, implementar um desenho próprio de perfil; não reutilizar ou apenas ajustar (achatando/escalando) o desenho frontal.